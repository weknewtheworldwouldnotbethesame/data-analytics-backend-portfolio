"""Run acceptance checks against the actual API and local HTTP receiver."""
import json, urllib.request, urllib.error, uuid, concurrent.futures
from pathlib import Path
BASE="http://localhost:5080"
checks=[]
def request(method,path,body=None):
    data=None if body is None else json.dumps(body).encode()
    req=urllib.request.Request(BASE+path,data=data,method=method,headers={"Content-Type":"application/json"})
    try:
        with urllib.request.urlopen(req,timeout=15) as response:
            raw=response.read()
            return response.status,json.loads(raw) if raw else None
    except urllib.error.HTTPError as error:
        return error.code,json.loads(error.read())
def check(name,condition):
    assert condition,name
    checks.append(name)
    print("PASS",name)
def payload(number):
    return {"orderNumber":number,"customerName":"Fictional customer","product":"Keyboard","quantity":2,"channel":"Web"}
token="VERIFY-"+uuid.uuid4().hex[:8]
ids=[]
try:
    code,data=request("POST","/api/orders",{})
    check("Missing fields return 400",code==400)
    bad=payload(token);bad["quantity"]=0
    check("Invalid quantity returns 400",request("POST","/api/orders",bad)[0]==400)
    code,order=request("POST","/api/orders",payload(token))
    oid=order["id"];ids.append(oid)
    check("Create returns 201 and a unique ID",code==201 and oid>0)
    check("Creation trace records received, validated and saved",[e["step"] for e in order["events"]]==["Received","Validated","Saved"])
    check("Duplicate order number returns 409",request("POST","/api/orders",payload(token.lower()))[0]==409)
    check("Unknown ID returns 404",request("GET","/api/orders/99999999")[0]==404)
    check("Retry refuses an unsent order",request("POST",f"/api/orders/{oid}/retry",{"scenario":"success"})[0]==409)
    bad=payload(token);bad["quantity"]=3
    code,updated=request("PUT",f"/api/orders/{oid}",bad)
    check("Update persists a changed quantity",code==200 and updated["quantity"]==3)
    code,failed=request("POST",f"/api/orders/{oid}/sync",{"scenario":"unavailable"})
    check("HTTP 503 creates a failed order and saved failure",code==200 and failed["status"]=="Failed" and failed["attempts"][0]["errorType"]=="HTTP 503")
    check("Failure retains response and timestamp",bool(failed["attempts"][0]["responseBody"]) and bool(failed["attempts"][0]["attemptedAt"]))
    code,recovered=request("POST",f"/api/orders/{oid}/retry",{"scenario":"success"})
    check("Retry recovers the failed order",code==200 and recovered["status"]=="Completed")
    check("Retry keeps earlier failure history",len(recovered["attempts"])==2 and recovered["attempts"][0]["outcome"]=="Failed")
    check("Completed order cannot be resent",request("POST",f"/api/orders/{oid}/sync",{"scenario":"success"})[0]==409)
    check("Completed order cannot be edited",request("PUT",f"/api/orders/{oid}",payload(token))[0]==409)
    for scenario,error_type in [("timeout","Timeout"),("invalid-response","Invalid response")]:
        code,item=request("POST","/api/orders",payload(token+"-"+scenario))
        ids.append(item["id"])
        code,result=request("POST",f"/api/orders/{item['id']}/sync",{"scenario":scenario})
        check(scenario+" is stored as a failure",code==200 and result["status"]=="Failed" and result["attempts"][0]["errorType"]==error_type)
    code,item=request("POST","/api/orders",payload(token+"-parallel"));ids.append(item["id"])
    with concurrent.futures.ThreadPoolExecutor(2) as pool:
        results=list(pool.map(lambda _:request("POST",f"/api/orders/{item['id']}/sync",{"scenario":"success"}),range(2)))
    check("Concurrent sends allow only one transfer",sorted(result[0] for result in results)==[200,409])
    check("Concurrent send creates one attempt",len(request("GET",f"/api/orders/{item['id']}")[1]["attempts"])==1)
    code,rows=request("GET","/api/analytics/orders")
    row=next(row for row in rows if row["orderId"]==oid)
    check("Analytics counts orders once and records recovery",row["attemptCount"]==2 and row["failureCount"]==1 and row["recovered"]==1 and row["resolutionMinutes"] is not None)
    check("Status filter returns only failed orders",all(order["status"]=="Failed" for order in request("GET","/api/orders?status=Failed")[1]))
    check("Order search finds the requested order",any(order["id"]==oid for order in request("GET","/api/orders?search="+token)[1]))
finally:
    for oid in ids:
        request("DELETE",f"/api/orders/{oid}")
if ids:
    check("Delete removes the order",request("GET",f"/api/orders/{ids[0]}")[0]==404)
Path("docs/acceptance-results.json").write_text(json.dumps({"environment":"Actual ASP.NET API, SQLite and local Postman receiver","passed":len(checks),"checks":checks},indent=2))

import json, pathlib
root=pathlib.Path(__file__).resolve().parents[1]
cases=[("Health","GET","/health",None,200),("Orders","GET","/api/orders",None,200),("Unknown order","GET","/api/orders/99999999",None,404),("Missing fields","POST","/api/orders",{},400),("Analytics","GET","/api/analytics/orders",None,200)]
items=[]
for name,method,path,body,status in cases:
    request={"method":method,"url":"{{baseUrl}}"+path,"header":[]}
    if body is not None:
        request["header"]=[{"key":"Content-Type","value":"application/json"}]
        request["body"]={"mode":"raw","raw":json.dumps(body)}
    script=['pm.test("Expected HTTP status", () => pm.response.to.have.status('+str(status)+'));']
    if name=="Analytics":
        script+=['pm.test("One row per order", () => { const rows=pm.response.json(); pm.expect(new Set(rows.map(r=>r.orderId)).size).to.eql(rows.length); });']
    items.append({"name":name,"request":request,"event":[{"listen":"test","script":{"type":"text/javascript","exec":script}}]})
collection={"info":{"name":"Order Sync API Checks","schema":"https://schema.getpostman.com/json/collection/v2.1.0/collection.json"},"variable":[{"key":"baseUrl","value":"http://localhost:5080"}],"item":items}
(root/"postman/collections/order-sync.postman_collection.json").write_text(json.dumps(collection,indent=2))

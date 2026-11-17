import json, pathlib
root=pathlib.Path(__file__).resolve().parents[1]
base=root/"powerbi"
def save(path,value):
    path.parent.mkdir(parents=True,exist_ok=True)
    path.write_text(json.dumps(value,indent=2),encoding="utf-8")
prefix="https://developer.microsoft.com/json-schemas/fabric/item/report/definition/"
save(base/"Order Sync.pbip",{"version":"1.0","artifacts":[{"report":{"path":"OrderSync.Report"}}],"settings":{"enableAutoRecovery":True}})
save(base/"OrderSync.Report/definition.pbir",{"version":"4.0","datasetReference":{"byPath":{"path":"../OrderSync.SemanticModel"}}})
save(base/"OrderSync.SemanticModel/definition.pbism",{"version":"4.0","settings":{}})
cols=[("OrderId","int64"),("OrderNumber","string"),("OrderDate","dateTime"),("Channel","string"),("Product","string"),("Quantity","int64"),("Status","string"),("AttemptCount","int64"),("FailureCount","int64"),("RetryCount","int64"),("Recovered","int64"),("LastFailureType","string"),("ResolutionMinutes","double"),("DataOrigin","string")]
measures={
"Total Orders":("COUNTROWS(Orders)","0"),
"Completed Orders":('CALCULATE([Total Orders], Orders[Status] = "Completed")',"0"),
"Open Failures":('CALCULATE([Total Orders], Orders[Status] = "Failed")',"0"),
"Completion Rate":("DIVIDE([Completed Orders], [Total Orders], 0)","0.0%"),
"Orders With Failures":("CALCULATE([Total Orders], Orders[FailureCount] > 0)","0"),
"Recovered Orders":("SUM(Orders[Recovered])","0"),
"Recovery Rate":("DIVIDE([Recovered Orders], [Orders With Failures], 0)","0.0%"),
"Retry Attempts":("SUM(Orders[RetryCount])","0"),
"Average Recovery Minutes":("AVERAGE(Orders[ResolutionMinutes])","0.0"),
"Open Failure Rate":("DIVIDE([Open Failures], [Total Orders], 0)","0.0%")
}
mtypes={"int64":"Int64.Type","string":"type text","dateTime":"type date","double":"type number"}
types=", ".join('{"'+name+'", '+mtypes[typ]+'}' for name,typ in cols)
source=['let','    Source = Csv.Document(File.Contents(DataFile), [Delimiter=",", Encoding=65001, QuoteStyle=QuoteStyle.Csv]),','    Headers = Table.PromoteHeaders(Source, [PromoteAllScalars=true]),','    Types = Table.TransformColumnTypes(Headers, {'+types+'}, "en-US")','in','    Types']
model={"name":"OrderSync","compatibilityLevel":1567,"model":{"culture":"en-US","defaultPowerBIDataSourceVersion":"powerBI_V3","sourceQueryCulture":"en-US","expressions":[{"name":"DataFile","kind":"m","expression":'"'+str(base/"data/orders.csv").replace("\\","/")+'" meta [IsParameterQuery=true, Type="Text", IsParameterQueryRequired=true]'}],"tables":[{"name":"Orders","columns":[dict(name=n,dataType=t,sourceColumn=n,summarizeBy="none",**({"formatString":"yyyy-MM-dd"} if t=="dateTime" else {})) for n,t in cols],"measures":[{"name":n,"expression":e,"formatString":f} for n,(e,f) in measures.items()],"partitions":[{"name":"Orders","mode":"import","source":{"type":"m","expression":source}}]}],"annotations":[{"name":"PBI_QueryOrder","value":'["DataFile","Orders"]'}]}}
save(base/"OrderSync.SemanticModel/model.bim",model)
definition=base/"OrderSync.Report/definition"
save(definition/"version.json",{"$schema":prefix+"versionMetadata/1.0.0/schema.json","version":"2.0.0"})
save(definition/"report.json",{"$schema":prefix+"report/2.0.0/schema.json","themeCollection":{"baseTheme":{"name":"CY24SU06","reportVersionAtImport":"5.37","type":"SharedResources"}}})
save(definition/"pages/pages.json",{"$schema":prefix+"pagesMetadata/1.0.0/schema.json","pageOrder":["overview","failures"],"activePageName":"overview"})
def literal(value):
    return {"expr":{"Literal":{"Value":value}}}
def field(name,measure=False):
    return {("Measure" if measure else "Column"):{"Expression":{"SourceRef":{"Entity":"Orders"}},"Property":name}}
def projection(name,measure=False):
    return {"field":field(name,measure),"queryRef":"Orders."+name,"nativeQueryRef":name}
def visual(page,name,typ,x,y,w,h,title,roles=None,objects=None):
    v={"$schema":prefix+"visualContainer/2.1.0/schema.json","name":name,"position":{"x":x,"y":y,"z":0,"width":w,"height":h,"tabOrder":0},"visual":{"visualType":typ,"drillFilterOtherVisuals":True,"visualContainerObjects":{"title":[{"properties":{"show":literal("true"),"text":literal("'"+title+"'"),"fontSize":literal("12D"),"fontColor":{"solid":{"color":literal("'#17334D'")}}}}],"background":[{"properties":{"show":literal("true"),"color":{"solid":{"color":literal("'#FFFFFF'")}},"transparency":literal("0D")}}]}}}
    if roles: v["visual"]["query"]={"queryState":{k:{"projections":[projection(n,m) for n,m in fields]} for k,fields in roles.items()}}
    if objects: v["visual"]["objects"]=objects
    save(definition/f"pages/{page}/visuals/{name}/visual.json",v)
def text(page,name,x,y,w,h,value,size=24):
    visual(page,name,"textbox",x,y,w,h,"",objects={"general":[{"properties":{"paragraphs":[{"textRuns":[{"value":value,"textStyle":{"fontSize":str(size)+"pt","fontFamily":"Segoe UI","color":"#17334D"}}]}]}}]})
for page,title in [("overview","Order performance"),("failures","Failure review")]:
    save(definition/f"pages/{page}/page.json",{"$schema":prefix+"page/2.0.0/schema.json","name":page,"displayName":title,"displayOption":"FitToPage","height":900,"width":1440})
text("overview","heading",24,12,1380,62,"ORDER SYNC  /  PERFORMANCE",26)
text("overview","caption",24,74,1380,45,"Fictional order data | Select a channel or a chart to filter the report.",12)
for i,measure in enumerate(["Total Orders","Completed Orders","Open Failures","Completion Rate","Recovery Rate"]):
    visual("overview","kpi"+str(i),"card",24+i*280,130,264,125,measure,{"Values":[(measure,True)]})
visual("overview","status","donutChart",24,280,400,275,"Current order status",{"Category":[("Status",False)],"Y":[("Total Orders",True)]})
visual("overview","trend","lineChart",444,280,690,275,"Orders and open failures by date",{"Category":[("OrderDate",False)],"Y":[("Total Orders",True),("Open Failures",True)]})
visual("overview","channel","slicer",1154,280,260,275,"Channel",{"Values":[("Channel",False)]})
visual("overview","reasons","clusteredBarChart",24,580,680,280,"Orders with failures by last failure reason",{"Category":[("LastFailureType",False)],"Y":[("Orders With Failures",True)]})
visual("overview","channels","clusteredColumnChart",724,580,690,280,"Completed and open failures by channel",{"Category":[("Channel",False)],"Y":[("Completed Orders",True),("Open Failures",True)]})
text("failures","heading2",24,12,1380,62,"ORDER SYNC  /  FAILURE REVIEW",26)
text("failures","caption2",24,74,1380,45,"Recovery includes completed orders with at least one earlier failed attempt.",12)
for i,measure in enumerate(["Orders With Failures","Recovered Orders","Retry Attempts","Average Recovery Minutes"]):
    visual("failures","reviewkpi"+str(i),"card",24+i*350,130,330,125,measure,{"Values":[(measure,True)]})
visual("failures","statusfilter","slicer",24,280,270,230,"Status",{"Values":[("Status",False)]})
visual("failures","originfilter","slicer",24,530,270,230,"Data origin",{"Values":[("DataOrigin",False)]})
visual("failures","details","tableEx",314,280,1100,580,"Order-level review",{"Values":[(n,False) for n in ["OrderNumber","OrderDate","Channel","Status","FailureCount","RetryCount","Recovered","LastFailureType","ResolutionMinutes"]]})
print(base/"Order Sync.pbip")

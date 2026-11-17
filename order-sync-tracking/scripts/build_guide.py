"""Create the project guide from verified application captures."""
from pathlib import Path
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor, white
from reportlab.lib.utils import ImageReader
from reportlab.platypus import Paragraph
from reportlab.lib.styles import ParagraphStyle

root = Path(__file__).resolve().parents[1]
out = root / 'output/pdf'
out.mkdir(parents=True, exist_ok=True)
pdf = canvas.Canvas(str(out / 'Order Sync - How It Works.pdf'), pagesize=(842, 595))
pdf.setTitle('Order Sync Failure Tracking - How It Works')
navy, teal, muted = map(HexColor, ['#17334D', '#087F8C', '#526577'])
style = ParagraphStyle('body', fontName='Helvetica', fontSize=12, leading=18, textColor=navy)

def para(text, x, y, width=746, size=12):
    local = ParagraphStyle('local', parent=style, fontSize=size, leading=size*1.5)
    p = Paragraph(text, local)
    w, h = p.wrap(width, 500)
    p.drawOn(pdf, x, y-h)
    return y-h-12

def page(number, title, subtitle):
    pdf.setFillColor(HexColor('#F5F8FB')); pdf.rect(0,0,842,595,fill=1,stroke=0)
    pdf.setFillColor(teal); pdf.rect(0,580,842,15,fill=1,stroke=0)
    pdf.setFillColor(navy); pdf.setFont('Helvetica-Bold',26); pdf.drawString(48,536,title)
    para(subtitle,48,511,size=11)
    pdf.setFillColor(muted); pdf.setFont('Helvetica',9)
    pdf.drawString(48,24,'ORDER SYNC FAILURE TRACKING  |  HOW IT WORKS')
    pdf.drawRightString(794,24,str(number))

def shot(name,x=48,y=56,w=746,h=365):
    image = ImageReader(str(root/'docs/screenshots'/name))
    iw,ih=image.getSize(); scale=min(w/iw,h/ih)
    pdf.drawImage(image,x+(w-iw*scale)/2,y+(h-ih*scale)/2,iw*scale,ih*scale,mask='auto')

page(1,'Order Sync Failure Tracking','Save the order. Track the transfer. Resolve the failure.')
y=para('An order may be saved but still fail while moving to another system. This project keeps the order, the error, and every transfer attempt together.',48,461,size=15)
for i,(name,body) in enumerate([
 ('NEXT.JS DASHBOARD','Create orders, search the queue, choose a transfer outcome, and open the history.'),
 ('C# API + SQLITE','Check the input, block duplicate order numbers, save the order, and record each attempt.'),
 ('HTTP RECEIVER','Confirm the matching order or return a controlled failure. A separate live Echo check confirms JSON transport.'),
 ('POWER BI','Read the exported order data. Compare completion, current failures, channels, and recovery.')]):
    yy=370-i*70
    pdf.setFillColor(teal); pdf.setFont('Helvetica-Bold',11); pdf.drawString(48,yy,name)
    para(body,238,yy+4,550,size=12)
para('The included records are fictional. The local receiver does not fulfill real orders.',48,64,size=10)
pdf.showPage()

page(2,'1. Start and send an order','Double-click Start Project.cmd, then open http://127.0.0.1:3000.')
para('Enter a new order number, customer, product, quantity, and channel. Click <b>Save order</b>. Choose <b>Confirm order</b>, then click <b>Send</b>. The status becomes <b>Completed</b>.',48,470)
shot('02-completed-order.png',h=340)
pdf.showPage()

page(3,'2. Find a failure and retry','Use a new order number for each new order.')
para('Save another order. Choose <b>Unavailable (503)</b>, then click <b>Send</b>. Read the saved error. Change the outcome to <b>Confirm order</b> and click <b>Retry</b>. A successful retry keeps the earlier failure in the history.',48,470)
shot('03-failed-order.png',h=340)
pdf.showPage()

page(4,'3. Read the order history','Click an order number to open its details.')
para('<b>Received</b>: input arrived.<br/><b>Validated</b>: required fields passed.<br/><b>Saved</b>: SQLite stored the order.<br/><b>Sent</b>: the API called the receiver.<br/><b>Failed</b>: an error was recorded.<br/><b>Completed</b>: the receiver confirmed the matching order.',48,460,275)
para('The two attempts show a failed send followed by a successful retry. Expand <b>Saved response</b> to inspect what the receiver returned.',48,245,275)
para('If a request times out, check the receiver before retrying because its outcome may be uncertain.',48,141,275,size=11)
shot('05-order-trace.png',x=340,y=48,w=454,h=425)
pdf.showPage()

page(5,'4. Read the performance report','Open powerbi/Order Sync.pbix. The PBIP folder also contains editable report source.')
para('<b>Total orders</b> counts each order once. <b>Completion rate</b> is completed orders divided by all orders. <b>Recovery rate</b> is recovered orders divided by orders that ever failed. Select a channel to filter the page; select it again to clear.',48,470)
shot('06-powerbi-overview.png',h=340)
pdf.showPage()

page(6,'5. Review failures and refresh','Use the Failure review tab to see order-level details.')
para('Filter by status or data origin. A recovered order has a completed status and at least one earlier failed attempt. Recovery time runs from the first failure to completion. A blank average means no recovered orders match the current filter.',48,470)
shot('07-powerbi-failures.png',y=94,h=300)
para('<b>Refresh:</b> run Refresh Power BI Data.cmd, then click Refresh in Power BI. The CSV is a snapshot. On another PC, change the DataFile parameter to that PC\'s powerbi/data/orders.csv path.',48,88,size=11)
pdf.showPage()

page(7,'6. Follow the code and checks','Read the files in this order. Each part has one clear job.')
y=460
for path,body in [
 ('backend/Models/Order.cs','The order fields, input rules, attempts, and history records.'),
 ('backend/Controllers/OrdersController.cs','Create, read, change, delete, send, and retry orders.'),
 ('backend/Services/SyncService.cs','Call the receiver, handle errors, and save the result.'),
 ('backend/Data/OrderDb.cs','Connect the order, attempt, and event tables in SQLite.'),
 ('backend/Controllers/AnalyticsController.cs','Build one row per order and export the report data.'),
 ('dashboard/app/page.js','Show the queue, order form, filters, and action buttons.')]:
    y=para('<b>'+path+'</b><br/>'+body,48,y,size=11)
para('<b>Verified:</b> 22 acceptance checks, 6 Postman assertions, connection failure, restart persistence, UTC timestamps, report totals, and report filters. The saved snapshot contains 45 orders, 35 completed, 6 failed, and 4 ready to send.',48,130,size=11)
para('<b>Independent review:</b> another person tested the project, confirmed by the project owner on November 17, 2026. Future work includes login, a real provider contract, provider-side duplicate protection, and scheduled report refresh.',48,65,size=10)
pdf.showPage()

page(8,'A short walkthrough','Use this order when explaining how the project works.')
y=455
for heading,body in [
 ('1. State the problem','An order can fail between systems. We need to find the failure and keep its history.'),
 ('2. Show a completed transfer','Create an order, choose Confirm order, and send it. Point to the Completed status.'),
 ('3. Show a failure and recovery','Send a new order with Unavailable selected. Read the error, then retry with Confirm order.'),
 ('4. Open the trace','Show both attempts and explain that the earlier failure was kept.'),
 ('5. Open Power BI','Explain total orders, current failures, completion rate, and recovery rate. Filter by channel.'),
 ('6. Refresh and connect the parts','Export the latest data, refresh Power BI, and explain dashboard -> API -> SQLite -> report.')]:
    y=para('<b>'+heading+'</b><br/>'+body,48,y,size=12)
para('The MP4 walkthrough uses captured application states. The screenshots and test results are in docs.',48,58,size=10)
pdf.save()
print(out/'Order Sync - How It Works.pdf')

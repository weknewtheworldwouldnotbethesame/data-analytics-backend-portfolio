"use client";
import { useEffect, useState } from "react";

export default function Dashboard() {
  const [orders, setOrders] = useState([]);
  const [status, setStatus] = useState("");
  const [search, setSearch] = useState("");
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState(false);
  const [scenario, setScenario] = useState("success");

  async function load() {
    try {
      const response = await fetch("/api/orders");
      if (!response.ok) throw new Error("Could not load orders.");
      setOrders(await response.json());
    } catch (error) { setMessage(error.message + " Start the API and refresh."); }
  }
  useEffect(() => { load(); }, []);

  async function create(event) {
    event.preventDefault();
    setBusy(true);
    const form = event.currentTarget;
    const input = Object.fromEntries(new FormData(form));
    input.quantity = Number(input.quantity);
    try {
      const response = await fetch("/api/orders", { method:"POST",
        headers:{"Content-Type":"application/json"}, body:JSON.stringify(input) });
      const result = await response.json();
      if (!response.ok) throw new Error(result.message || Object.values(result.errors || {}).flat().join(" ") || "Check the order fields.");
      setMessage("Order " + result.orderNumber + " saved. Select Send to transfer it.");
      form.reset(); await load();
    } catch (error) { setMessage(error.message); }
    finally { setBusy(false); }
  }

  async function send(order) {
    setBusy(true);
    try {
      const action = order.status === "Failed" ? "retry" : "sync";
      const response = await fetch("/api/orders/" + order.id + "/" + action, {
        method:"POST", headers:{"Content-Type":"application/json"}, body:JSON.stringify({scenario}) });
      const result = await response.json();
      if (!response.ok) throw new Error(result.message || "Send failed.");
      setMessage(result.orderNumber + ": " + result.status + (result.lastError ? " - " + result.lastError : ""));
      await load();
    } catch (error) { setMessage(error.message); }
    finally { setBusy(false); }
  }

  const visible = orders.filter(order => (!status || order.status === status) &&
    (order.orderNumber.toLowerCase().includes(search.toLowerCase()) || String(order.id) === search));
  const count = value => orders.filter(order => order.status === value).length;

  return <main>
    <header><div><p className="eyebrow">OPERATIONS / ORDER TRANSFER</p><h1>Order Sync</h1>
      <p>Track every transfer. Find the failure. Confirm the recovery.</p></div>
      <a className="button" href="/api/analytics/export">Export for Power BI ↗</a></header>
    <div className="notice">Local receiver · Fictional order data · All timestamps shown in your local time</div>
    <section className="metrics">
      {[["Total orders",orders.length],["Completed",count("Completed")],["Needs attention",count("Failed")],["Ready to send",count("Received")]].map(([name,value]) =>
        <article key={name}><span>{name}</span><strong>{value}</strong></article>)}
    </section>
    <section className="panel"><h2>Create an order</h2>
      <form onSubmit={create}>
        <label>Order number<input name="orderNumber" placeholder="OS-2001" maxLength="40" required /></label>
        <label>Customer<input name="customerName" placeholder="Customer 43" maxLength="80" required /></label>
        <label>Product<input name="product" placeholder="Keyboard" maxLength="80" required /></label>
        <label>Quantity<input name="quantity" type="number" min="1" max="10000" defaultValue="1" required /></label>
        <label>Channel<select name="channel"><option>Web</option><option>Store</option><option>Partner</option></select></label>
        <button disabled={busy}>Save order</button>
      </form>
    </section>
    <section className="panel">
      <div className="toolbar"><h2>Order queue</h2>
        <input aria-label="Search orders" placeholder="Search ID or order number" value={search} onChange={event=>setSearch(event.target.value)} />
        <select aria-label="Filter status" value={status} onChange={event=>setStatus(event.target.value)}>
          <option value="">All statuses</option><option>Received</option><option>Completed</option><option>Failed</option></select>
        <label>Receiver outcome <select value={scenario} onChange={event=>setScenario(event.target.value)}>
          <option value="success">Confirm order</option><option value="unavailable">Unavailable (503)</option>
          <option value="timeout">Timeout</option><option value="invalid-response">Invalid response</option></select></label>
      </div>
      <p role="status" className="message">{busy ? "Processing order…" : message}</p>
      <div className="tablewrap"><table><thead><tr><th>Order</th><th>Created</th><th>Channel</th><th>Product</th><th>Status</th><th>Action</th></tr></thead>
        <tbody>{visible.map(order=><tr key={order.id}>
          <td><a href={"/orders/"+order.id}>{order.orderNumber}</a><small>#{order.id}</small></td>
          <td>{new Date(order.createdAt).toLocaleString()}</td><td>{order.channel}</td><td>{order.product}</td>
          <td><span className={"pill "+order.status}>{order.status}</span></td>
          <td>{["Received","Failed"].includes(order.status) ? <button disabled={busy} onClick={()=>send(order)}>{order.status==="Failed"?"Retry":"Send"}</button>:<a href={"/orders/"+order.id}>View trace →</a>}</td>
        </tr>)}</tbody></table></div>
      {visible.length===0 && <p>No matching orders.</p>}
    </section>
    <footer>ASP.NET Core + SQLite + Next.js + Power BI <a href="http://localhost:5080/swagger" target="_blank">API explorer ↗</a></footer>
  </main>;
}

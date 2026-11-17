"use client";
import { use, useEffect, useState } from "react";

export default function Details({ params }) {
  const { id } = use(params);
  const [order, setOrder] = useState(null);
  const [error, setError] = useState("");
  useEffect(() => {
    fetch("/api/orders/"+id).then(async response => {
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || "Unable to load order.");
      setOrder(data);
    }).catch(error => setError(error.message));
  }, [id]);
  if (!order) return <main><a href="/">← Order queue</a><p>{error || "Opening order…"}</p></main>;
  return <main>
    <a href="/">← Order queue</a>
    <header><div><p className="eyebrow">ORDER DETAILS</p><h1>{order.orderNumber}</h1>
      <p>{order.customerName} · {order.product} × {order.quantity} · {order.channel}</p></div>
      <span className={"pill "+order.status}>{order.status}</span></header>
    <p className="notice">{order.dataOrigin}</p>
    {order.lastError && <p className="error">{order.lastError}</p>}
    <section className="panel"><h2>Order trace</h2><ol className="trace">
      {order.events.map(item=><li key={item.id}><strong>{item.step}</strong><time>{new Date(item.at).toLocaleString()}</time><p>{item.message}</p></li>)}
    </ol></section>
    <section className="panel"><h2>Transfer attempts</h2>
      {order.attempts.length===0 && <p>No send has been requested.</p>}
      {order.attempts.map((attempt,index)=><article className="attempt" key={attempt.id}>
        <strong>Attempt {index+1} · {attempt.outcome}</strong><span>{attempt.durationMs} ms</span>
        <p>{attempt.errorMessage || "Receiver confirmed the order."}</p>
        <details><summary>Saved response</summary><pre>{attempt.responseBody || "No response received."}</pre></details>
      </article>)}
    </section>
  </main>;
}

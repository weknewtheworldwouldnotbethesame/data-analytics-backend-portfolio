export default function OrderTable({ orders, onRetry }) {
  return (
    <section aria-label="Order status">
      <h2>Orders</h2>
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Order ID</th>
              <th>Status</th>
              <th>Failure reason</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.id}>
                <td>#{order.id}</td>
                <td>
                  <span className={`status ${order.status.toLowerCase()}`}>
                    {order.status}
                  </span>
                </td>
                <td>{order.error}</td>
                <td>
                  {order.status === 'Failed' ? (
                    <button onClick={() => onRetry(order.id)}>Queue retry</button>
                  ) : (
                    '—'
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="note">Queue retry changes a failed order to Pending in the local display.</p>
    </section>
  );
}

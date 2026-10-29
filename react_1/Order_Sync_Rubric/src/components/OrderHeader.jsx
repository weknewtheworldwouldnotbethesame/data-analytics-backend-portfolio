export default function OrderHeader({ title }) {
  return (
    <header className="order-header">
      <img
        className="order-image"
        src="/order-sync.svg"
        alt="Two arrows representing order synchronization"
      />
      <div>
        <h1>{title}</h1>
        <p>Order processing status and failure details</p>
      </div>
    </header>
  );
}

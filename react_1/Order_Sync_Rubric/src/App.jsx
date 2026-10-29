import { useState } from 'react';
import OrderHeader from './components/OrderHeader.jsx';
import OrderTable from './components/OrderTable.jsx';
import './App.css';

const startingOrders = [
  { id: '1001', status: 'Synced', error: 'None' },
  { id: '1002', status: 'Failed', error: 'Connection timeout' },
  { id: '1003', status: 'Pending', error: 'None' },
];

export default function App() {
  // React Hook: updates the order list on screen.
  const [orders, setOrders] = useState(startingOrders);

  function retryOrder(orderId) {
    setOrders((currentOrders) =>
      currentOrders.map((order) =>
        order.id === orderId
          ? { ...order, status: 'Pending', error: 'None' }
          : order
      )
    );
  }

  return (
    <main className="app">
      <OrderHeader title="Order Sync Failure Tracking" />
      <OrderTable orders={orders} onRetry={retryOrder} />
    </main>
  );
}

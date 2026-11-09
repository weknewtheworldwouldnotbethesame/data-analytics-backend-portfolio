SELECT tickets.ticket_id, users.username, tickets.title, tickets.status
FROM tickets
JOIN users ON tickets.user_id = users.user_id
ORDER BY tickets.ticket_id;
	
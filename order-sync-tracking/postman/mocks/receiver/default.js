const http = require("http");
// This receiver echoes JSON; it does not place or fulfill real orders.
http.createServer((req, res) => {
  if (req.method !== "POST" || req.url.split("?")[0] !== "/post") {
    res.writeHead(501); res.end("Route not modeled"); return;
  }
  function respond(body) {
    res.setHeader("Content-Type", "application/json");
    if (body.scenario === "unavailable") {
      res.writeHead(503); res.end(JSON.stringify({error:"Receiver unavailable"})); return;
    }
    if (body.scenario === "invalid-response") {
      res.end(JSON.stringify({message:"Missing confirmation"})); return;
    }
    const send = () => res.end(JSON.stringify({json:body, data:body}));
    if (body.scenario === "timeout") setTimeout(send, 4500);
    else send();
  }
  if (req.body !== undefined) {
    respond(typeof req.body === "string" ? JSON.parse(req.body) : req.body);
  } else {
    let raw = "";
    req.on("data", chunk => raw += chunk);
    req.on("end", () => { try { respond(JSON.parse(raw)); } catch { res.writeHead(400); res.end("{}"); } });
  }
}).listen(process.env.PORT || 4011);

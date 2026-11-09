--
-- PostgreSQL database dump
--

\restrict SKrFMpGs1CY9nZZvbGETUfdr5g5hlnQFH2ZIvigO1t1IevvHedPpNsKZ0mlmrWU

-- Dumped from database version 18.6
-- Dumped by pg_dump version 18.6

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: tickets; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.tickets (
    ticket_id integer NOT NULL,
    user_id integer NOT NULL,
    title text NOT NULL,
    status character varying(30) NOT NULL
);


--
-- Name: users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.users (
    user_id integer NOT NULL,
    username character varying(50) NOT NULL,
    user_type character varying(20) NOT NULL
);


--
-- Data for Name: tickets; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.tickets (ticket_id, user_id, title, status) FROM stdin;
2	1	DAE WIFI IS VERY SLOW PLEASE 	Closed
3	1	my wifi is not working, I believe is the aliens	New
4	1	My router is down	New
5	1	my pc doesnt work please fix	New
6	1	my laptop is broken	Closed
7	1	MY AMAZON IS NOT WORKING	Closed
8	1	MY LAPTOP IS NOT WORKING IS BLANK BUTTONS ARE BROKEN	Closed
9	1	MY LAPTOP BROKE AND I AM BROKE	Closed
10	1	MY PC IS NOT WORKING I THINK IS DRUNK	Closed
1	1	Computer froze	Closed
12	6	Printer is not connecting to Wi-Fi	New
13	7	Email application will not open	New
14	8	Monitor displays a blank screen	Closed
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.users (user_id, username, user_type) FROM stdin;
1	client1	client
2	client2	client
3	worker1	worker
4	worker2	worker
5	manager1	manager
6	client3	client
7	client4	client
8	client5	client
\.


--
-- Name: tickets tickets_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_pkey PRIMARY KEY (ticket_id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (user_id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- Name: tickets tickets_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id);


--
-- PostgreSQL database dump complete
--

\unrestrict SKrFMpGs1CY9nZZvbGETUfdr5g5hlnQFH2ZIvigO1t1IevvHedPpNsKZ0mlmrWU


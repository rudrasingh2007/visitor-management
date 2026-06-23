--
-- PostgreSQL database dump
--

\restrict zAzPEyvK6pD4biTK2cPUJKfbpAxRskacYEY7b80IIl5RcjTwQC2RZTeXRESURqU

-- Dumped from database version 18.4
-- Dumped by pg_dump version 18.4

-- Started on 2026-06-23 13:01:16

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
-- TOC entry 230 (class 1259 OID 16734)
-- Name: AppointmentMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AppointmentMaster" (
    "AppointmentId" integer NOT NULL,
    "VisitorId" integer NOT NULL,
    "DepartmentId" integer NOT NULL,
    "EmployeeId" integer NOT NULL,
    "AppointmentDate" date NOT NULL,
    "AppointmentTime" time without time zone NOT NULL,
    "Purpose" character varying(250) NOT NULL,
    "Remarks" character varying(500),
    "Status" character varying(15) DEFAULT 'Pending'::character varying NOT NULL,
    "CreatedDate" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public."AppointmentMaster" OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 16733)
-- Name: AppointmentMaster_AppointmentId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."AppointmentMaster_AppointmentId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."AppointmentMaster_AppointmentId_seq" OWNER TO postgres;

--
-- TOC entry 5193 (class 0 OID 0)
-- Dependencies: 229
-- Name: AppointmentMaster_AppointmentId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."AppointmentMaster_AppointmentId_seq" OWNED BY public."AppointmentMaster"."AppointmentId";


--
-- TOC entry 228 (class 1259 OID 16712)
-- Name: DepartmentMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."DepartmentMaster" (
    "DepartmentId" integer NOT NULL,
    "DepartmentName" character varying(100) NOT NULL,
    "Description" character varying(250),
    "Status" character varying(15) DEFAULT 'Active'::character varying NOT NULL,
    "CreatedDate" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public."DepartmentMaster" OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 16711)
-- Name: DepartmentMaster_DepartmentId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."DepartmentMaster_DepartmentId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."DepartmentMaster_DepartmentId_seq" OWNER TO postgres;

--
-- TOC entry 5194 (class 0 OID 0)
-- Dependencies: 227
-- Name: DepartmentMaster_DepartmentId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."DepartmentMaster_DepartmentId_seq" OWNED BY public."DepartmentMaster"."DepartmentId";


--
-- TOC entry 226 (class 1259 OID 16684)
-- Name: EmployeeMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."EmployeeMaster" (
    "EmployeeId" integer NOT NULL,
    "EmployeeCode" character varying(50) NOT NULL,
    "FirstName" character varying(50) NOT NULL,
    "LastName" character varying(50),
    "Gender" character varying(15) NOT NULL,
    "Designation" character varying(100) NOT NULL,
    "Email" character varying(100) NOT NULL,
    "MobileNumber" character varying(15) NOT NULL,
    "Status" character varying(15) DEFAULT 'Active'::character varying NOT NULL,
    "CreatedDate" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "DepartmentId" integer DEFAULT 1 NOT NULL
);


ALTER TABLE public."EmployeeMaster" OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 16683)
-- Name: EmployeeMaster_EmployeeId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."EmployeeMaster_EmployeeId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."EmployeeMaster_EmployeeId_seq" OWNER TO postgres;

--
-- TOC entry 5195 (class 0 OID 0)
-- Dependencies: 225
-- Name: EmployeeMaster_EmployeeId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."EmployeeMaster_EmployeeId_seq" OWNED BY public."EmployeeMaster"."EmployeeId";


--
-- TOC entry 240 (class 1259 OID 24618)
-- Name: EntryRequestMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."EntryRequestMaster" (
    "EntryRequestId" integer NOT NULL,
    "VisitorId" integer NOT NULL,
    "DepartmentId" integer NOT NULL,
    "EmployeeId" integer NOT NULL,
    "Purpose" character varying(250) NOT NULL,
    "RequestDateTime" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "ApprovalStatus" character varying(25) DEFAULT 'Pending'::character varying NOT NULL,
    "ApprovalRemarks" character varying(500),
    "ApprovedByEmployeeId" integer,
    "ApprovalDateTime" timestamp with time zone,
    "CreatedByUserId" integer NOT NULL,
    "CreatedDate" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public."EntryRequestMaster" OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 24617)
-- Name: EntryRequestMaster_EntryRequestId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."EntryRequestMaster_EntryRequestId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."EntryRequestMaster_EntryRequestId_seq" OWNER TO postgres;

--
-- TOC entry 5196 (class 0 OID 0)
-- Dependencies: 239
-- Name: EntryRequestMaster_EntryRequestId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."EntryRequestMaster_EntryRequestId_seq" OWNED BY public."EntryRequestMaster"."EntryRequestId";


--
-- TOC entry 234 (class 1259 OID 16808)
-- Name: GatePassMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."GatePassMaster" (
    "GatePassId" integer NOT NULL,
    "GatePassNumber" character varying(50) NOT NULL,
    "VisitEntryId" integer,
    "VisitorId" integer NOT NULL,
    "EmployeeId" integer NOT NULL,
    "DepartmentId" integer NOT NULL,
    "IssueDateTime" timestamp with time zone NOT NULL,
    "ExpiryDateTime" timestamp with time zone NOT NULL,
    "QRCodePath" character varying(250) NOT NULL,
    "Status" character varying(15) DEFAULT 'Active'::character varying NOT NULL,
    "CreatedDate" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "EntryRequestId" integer,
    "AppointmentId" integer
);


ALTER TABLE public."GatePassMaster" OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 16807)
-- Name: GatePassMaster_GatePassId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."GatePassMaster_GatePassId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."GatePassMaster_GatePassId_seq" OWNER TO postgres;

--
-- TOC entry 5197 (class 0 OID 0)
-- Dependencies: 233
-- Name: GatePassMaster_GatePassId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."GatePassMaster_GatePassId_seq" OWNED BY public."GatePassMaster"."GatePassId";


--
-- TOC entry 236 (class 1259 OID 24577)
-- Name: ModuleMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ModuleMaster" (
    "ModuleId" integer NOT NULL,
    "ModuleName" character varying(100) NOT NULL
);


ALTER TABLE public."ModuleMaster" OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 24576)
-- Name: ModuleMaster_ModuleId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."ModuleMaster_ModuleId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."ModuleMaster_ModuleId_seq" OWNER TO postgres;

--
-- TOC entry 5198 (class 0 OID 0)
-- Dependencies: 235
-- Name: ModuleMaster_ModuleId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."ModuleMaster_ModuleId_seq" OWNED BY public."ModuleMaster"."ModuleId";


--
-- TOC entry 220 (class 1259 OID 16584)
-- Name: RoleMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."RoleMaster" (
    "RoleId" integer NOT NULL,
    "RoleName" character varying(50) NOT NULL,
    "Description" character varying(250),
    "Status" character varying(15) NOT NULL,
    "CreatedDate" timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public."RoleMaster" OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 16583)
-- Name: RoleMaster_RoleId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."RoleMaster_RoleId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."RoleMaster_RoleId_seq" OWNER TO postgres;

--
-- TOC entry 5199 (class 0 OID 0)
-- Dependencies: 219
-- Name: RoleMaster_RoleId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."RoleMaster_RoleId_seq" OWNED BY public."RoleMaster"."RoleId";


--
-- TOC entry 238 (class 1259 OID 24588)
-- Name: RoleRights; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."RoleRights" (
    "RoleRightId" integer NOT NULL,
    "RoleId" integer NOT NULL,
    "ModuleId" integer NOT NULL,
    "CanView" boolean DEFAULT false NOT NULL,
    "CanAdd" boolean DEFAULT false NOT NULL,
    "CanEdit" boolean DEFAULT false NOT NULL,
    "CanDelete" boolean DEFAULT false NOT NULL
);


ALTER TABLE public."RoleRights" OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 24587)
-- Name: RoleRights_RoleRightId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."RoleRights_RoleRightId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."RoleRights_RoleRightId_seq" OWNER TO postgres;

--
-- TOC entry 5200 (class 0 OID 0)
-- Dependencies: 237
-- Name: RoleRights_RoleRightId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."RoleRights_RoleRightId_seq" OWNED BY public."RoleRights"."RoleRightId";


--
-- TOC entry 222 (class 1259 OID 16597)
-- Name: UserMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."UserMaster" (
    "UserId" integer NOT NULL,
    "Username" character varying(50) NOT NULL,
    "FullName" character varying(100) NOT NULL,
    "Email" character varying(100) NOT NULL,
    "MobileNumber" character varying(15) NOT NULL,
    "Password" character varying(100) NOT NULL,
    "RoleId" integer NOT NULL,
    "Status" character varying(15) NOT NULL,
    "CreatedDate" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "LastUpdatedDate" timestamp with time zone,
    "EmployeeId" integer
);


ALTER TABLE public."UserMaster" OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 16596)
-- Name: UserMaster_UserId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."UserMaster_UserId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."UserMaster_UserId_seq" OWNER TO postgres;

--
-- TOC entry 5201 (class 0 OID 0)
-- Dependencies: 221
-- Name: UserMaster_UserId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."UserMaster_UserId_seq" OWNED BY public."UserMaster"."UserId";


--
-- TOC entry 232 (class 1259 OID 16769)
-- Name: VisitEntryMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."VisitEntryMaster" (
    "VisitEntryId" integer NOT NULL,
    "AppointmentId" integer,
    "VisitorId" integer NOT NULL,
    "EmployeeId" integer NOT NULL,
    "DepartmentId" integer NOT NULL,
    "CheckInTime" timestamp with time zone,
    "CheckOutTime" timestamp with time zone,
    "VisitStatus" character varying(20) DEFAULT 'Checked In'::character varying NOT NULL,
    "Remarks" character varying(500),
    "CreatedDate" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "EntryRequestId" integer
);


ALTER TABLE public."VisitEntryMaster" OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 16768)
-- Name: VisitEntryMaster_VisitEntryId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."VisitEntryMaster_VisitEntryId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."VisitEntryMaster_VisitEntryId_seq" OWNER TO postgres;

--
-- TOC entry 5202 (class 0 OID 0)
-- Dependencies: 231
-- Name: VisitEntryMaster_VisitEntryId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."VisitEntryMaster_VisitEntryId_seq" OWNED BY public."VisitEntryMaster"."VisitEntryId";


--
-- TOC entry 224 (class 1259 OID 16622)
-- Name: VisitorMaster; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."VisitorMaster" (
    "VisitorId" integer NOT NULL,
    "FirstName" character varying(50) NOT NULL,
    "LastName" character varying(50),
    "Gender" character varying(15) NOT NULL,
    "MobileNumber" character varying(15) NOT NULL,
    "Email" character varying(100) NOT NULL,
    "Address" character varying(250),
    "City" character varying(50),
    "State" character varying(50),
    "CompanyName" character varying(100),
    "PhotoPath" character varying(250),
    "IdProofType" character varying(50) NOT NULL,
    "IdProofNumber" character varying(50) NOT NULL,
    "Status" character varying(15) NOT NULL,
    "CreatedDate" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "LastUpdatedDate" timestamp with time zone
);


ALTER TABLE public."VisitorMaster" OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 16621)
-- Name: VisitorMaster_VisitorId_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."VisitorMaster_VisitorId_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public."VisitorMaster_VisitorId_seq" OWNER TO postgres;

--
-- TOC entry 5203 (class 0 OID 0)
-- Dependencies: 223
-- Name: VisitorMaster_VisitorId_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."VisitorMaster_VisitorId_seq" OWNED BY public."VisitorMaster"."VisitorId";


--
-- TOC entry 241 (class 1259 OID 24668)
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- TOC entry 4923 (class 2604 OID 16737)
-- Name: AppointmentMaster AppointmentId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AppointmentMaster" ALTER COLUMN "AppointmentId" SET DEFAULT nextval('public."AppointmentMaster_AppointmentId_seq"'::regclass);


--
-- TOC entry 4920 (class 2604 OID 16715)
-- Name: DepartmentMaster DepartmentId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DepartmentMaster" ALTER COLUMN "DepartmentId" SET DEFAULT nextval('public."DepartmentMaster_DepartmentId_seq"'::regclass);


--
-- TOC entry 4916 (class 2604 OID 16687)
-- Name: EmployeeMaster EmployeeId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EmployeeMaster" ALTER COLUMN "EmployeeId" SET DEFAULT nextval('public."EmployeeMaster_EmployeeId_seq"'::regclass);


--
-- TOC entry 4938 (class 2604 OID 24621)
-- Name: EntryRequestMaster EntryRequestId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EntryRequestMaster" ALTER COLUMN "EntryRequestId" SET DEFAULT nextval('public."EntryRequestMaster_EntryRequestId_seq"'::regclass);


--
-- TOC entry 4929 (class 2604 OID 16811)
-- Name: GatePassMaster GatePassId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GatePassMaster" ALTER COLUMN "GatePassId" SET DEFAULT nextval('public."GatePassMaster_GatePassId_seq"'::regclass);


--
-- TOC entry 4932 (class 2604 OID 24580)
-- Name: ModuleMaster ModuleId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ModuleMaster" ALTER COLUMN "ModuleId" SET DEFAULT nextval('public."ModuleMaster_ModuleId_seq"'::regclass);


--
-- TOC entry 4910 (class 2604 OID 16587)
-- Name: RoleMaster RoleId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RoleMaster" ALTER COLUMN "RoleId" SET DEFAULT nextval('public."RoleMaster_RoleId_seq"'::regclass);


--
-- TOC entry 4933 (class 2604 OID 24591)
-- Name: RoleRights RoleRightId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RoleRights" ALTER COLUMN "RoleRightId" SET DEFAULT nextval('public."RoleRights_RoleRightId_seq"'::regclass);


--
-- TOC entry 4912 (class 2604 OID 16600)
-- Name: UserMaster UserId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserMaster" ALTER COLUMN "UserId" SET DEFAULT nextval('public."UserMaster_UserId_seq"'::regclass);


--
-- TOC entry 4926 (class 2604 OID 16772)
-- Name: VisitEntryMaster VisitEntryId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitEntryMaster" ALTER COLUMN "VisitEntryId" SET DEFAULT nextval('public."VisitEntryMaster_VisitEntryId_seq"'::regclass);


--
-- TOC entry 4914 (class 2604 OID 16625)
-- Name: VisitorMaster VisitorId; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitorMaster" ALTER COLUMN "VisitorId" SET DEFAULT nextval('public."VisitorMaster_VisitorId_seq"'::regclass);


--
-- TOC entry 5176 (class 0 OID 16734)
-- Dependencies: 230
-- Data for Name: AppointmentMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AppointmentMaster" ("AppointmentId", "VisitorId", "DepartmentId", "EmployeeId", "AppointmentDate", "AppointmentTime", "Purpose", "Remarks", "Status", "CreatedDate") FROM stdin;
30	6	2	2	2026-06-20	23:00:00	meeting	\N	Checked Out	2026-06-19 12:41:57.591077+05:30
31	6	2	2	2026-06-19	16:30:00	meeting	\N	Checked Out	2026-06-19 16:23:04.29256+05:30
32	6	2	2	2026-06-19	17:20:00	meeting	\N	Checked Out	2026-06-19 17:19:07.802435+05:30
33	6	4	5	2026-06-23	11:55:00	interview	\N	Checked Out	2026-06-23 11:54:11.757539+05:30
\.


--
-- TOC entry 5174 (class 0 OID 16712)
-- Dependencies: 228
-- Data for Name: DepartmentMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."DepartmentMaster" ("DepartmentId", "DepartmentName", "Description", "Status", "CreatedDate") FROM stdin;
1	HR	Human Resources Department	Active	2026-06-16 16:58:58.761632+05:30
2	IT/Technical	Information Technology Department	Active	2026-06-16 16:58:58.761632+05:30
3	Finance	Accounts and Finance Department	Active	2026-06-16 16:58:58.761632+05:30
4	Operations	Operations Management	Active	2026-06-16 16:58:58.761632+05:30
5	Sales/Marketing	Sales and Marketing Team	Active	2026-06-16 16:58:58.761632+05:30
6	Administration	Office Administration	Active	2026-06-16 16:58:58.761632+05:30
\.


--
-- TOC entry 5172 (class 0 OID 16684)
-- Dependencies: 226
-- Data for Name: EmployeeMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."EmployeeMaster" ("EmployeeId", "EmployeeCode", "FirstName", "LastName", "Gender", "Designation", "Email", "MobileNumber", "Status", "CreatedDate", "DepartmentId") FROM stdin;
5	emp1004	Rudra	Singh	Male	Operations Manager	rudra123@gmail.com	6783478923	Active	2026-06-17 14:49:27.59021+05:30	4
3	emp1002	Alex	Joseph	Male	Sales Manager	joseph9898@gmail.com	9878675478	Active	2026-06-17 10:56:38.068566+05:30	5
4	emp1003	Nakul	Chaudhary	Male	Finance Manager	nakulchaudhary1234@gmail.com	6573894782	Active	2026-06-17 11:33:47.209944+05:30	3
2	emp1001	rohit	sharma	Male	software engineer	rohit123@gmail.com	1234567789	Active	2026-06-16 17:31:25.270672+05:30	2
\.


--
-- TOC entry 5186 (class 0 OID 24618)
-- Dependencies: 240
-- Data for Name: EntryRequestMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."EntryRequestMaster" ("EntryRequestId", "VisitorId", "DepartmentId", "EmployeeId", "Purpose", "RequestDateTime", "ApprovalStatus", "ApprovalRemarks", "ApprovedByEmployeeId", "ApprovalDateTime", "CreatedByUserId", "CreatedDate") FROM stdin;
23	7	3	4	meetrng	2026-06-19 13:07:40.800425+05:30	Approved	Approved	4	2026-06-19 13:08:36.806237+05:30	3	2026-06-19 13:07:40.800425+05:30
24	7	3	4	interview	2026-06-19 16:36:15.997723+05:30	Approved	Approved	4	2026-06-19 16:37:13.947463+05:30	3	2026-06-19 16:36:15.997767+05:30
25	7	3	4	meeting	2026-06-19 16:47:21.135322+05:30	Approved	Approved	4	2026-06-19 16:47:33.923298+05:30	3	2026-06-19 16:47:21.135375+05:30
26	7	2	2	meeting	2026-06-19 17:42:11.759574+05:30	Approved	Approved	2	2026-06-19 17:43:29.718209+05:30	3	2026-06-19 17:42:11.7596+05:30
27	7	3	4	ok	2026-06-19 17:49:00.382163+05:30	Approved	ok	\N	2026-06-19 17:49:06.654454+05:30	3	2026-06-19 17:49:00.382203+05:30
28	6	5	3	enter	2026-06-19 17:53:54.164444+05:30	Approved	ok	\N	2026-06-19 17:54:00.450537+05:30	3	2026-06-19 17:53:54.164466+05:30
29	6	4	5	meeting	2026-06-23 12:25:08.038183+05:30	Approved	Approved	5	2026-06-23 12:25:23.301354+05:30	3	2026-06-23 12:25:08.038227+05:30
\.


--
-- TOC entry 5180 (class 0 OID 16808)
-- Dependencies: 234
-- Data for Name: GatePassMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."GatePassMaster" ("GatePassId", "GatePassNumber", "VisitEntryId", "VisitorId", "EmployeeId", "DepartmentId", "IssueDateTime", "ExpiryDateTime", "QRCodePath", "Status", "CreatedDate", "EntryRequestId", "AppointmentId") FROM stdin;
20	GP-20260619-30	25	6	2	2	2026-06-19 12:57:09.087678+05:30	2026-06-20 12:57:09.087702+05:30	/uploads/qrcodes/GP-20260619-30.png	Checked Out	2026-06-19 12:57:09.087871+05:30	\N	30
22	GP-20260619-23	26	7	4	3	2026-06-19 13:09:26.660275+05:30	2026-06-20 13:09:26.660275+05:30	/uploads/qrcodes/GP-20260619-23.png	Checked Out	2026-06-19 13:09:26.66028+05:30	23	\N
24	GP-20260619-31	27	6	2	2	2026-06-19 16:25:29.087593+05:30	2026-06-20 16:25:29.087622+05:30	/uploads/qrcodes/GP-20260619-31.png	Checked Out	2026-06-19 16:25:29.087713+05:30	\N	31
26	GP-20260619-24	29	7	4	3	2026-06-19 16:38:43.675183+05:30	2026-06-20 16:38:43.675183+05:30	/uploads/qrcodes/GP-20260619-24.png	Checked Out	2026-06-19 16:38:43.675185+05:30	24	\N
27	GP-20260619-25	30	7	4	3	2026-06-19 16:48:02.855979+05:30	2026-06-20 16:48:02.856017+05:30	/uploads/qrcodes/GP-20260619-25.png	Checked Out	2026-06-19 16:48:02.85613+05:30	25	\N
28	GP-20260619-32	31	6	2	2	2026-06-19 17:28:38.976544+05:30	2026-06-20 17:28:38.976566+05:30	/uploads/qrcodes/GP-20260619-32.png	Checked Out	2026-06-19 17:28:38.976615+05:30	\N	32
29	GP-20260619-26	32	7	2	2	2026-06-19 17:44:04.087674+05:30	2026-06-20 17:44:04.087675+05:30	/uploads/qrcodes/GP-20260619-26.png	Checked Out	2026-06-19 17:44:04.087677+05:30	26	\N
30	GP-20260619-27	33	7	4	3	2026-06-19 17:49:40.778982+05:30	2026-06-20 17:49:40.779014+05:30	/uploads/qrcodes/GP-20260619-27.png	Checked Out	2026-06-19 17:49:40.779096+05:30	27	\N
31	GP-20260619-28	34	6	3	5	2026-06-19 17:54:11.5124+05:30	2026-06-20 17:54:11.51242+05:30	/uploads/qrcodes/GP-20260619-28.png	Checked Out	2026-06-19 17:54:11.512486+05:30	28	\N
32	GP-20260623-33	35	6	5	4	2026-06-23 12:09:48.230392+05:30	2026-06-24 12:09:48.230419+05:30	/uploads/qrcodes/GP-20260623-33.png	Checked Out	2026-06-23 12:09:48.2305+05:30	\N	33
33	GP-20260623-29	36	6	5	4	2026-06-23 12:26:00.485362+05:30	2026-06-24 12:26:00.485382+05:30	/uploads/qrcodes/GP-20260623-29.png	Checked Out	2026-06-23 12:26:00.485449+05:30	29	\N
\.


--
-- TOC entry 5182 (class 0 OID 24577)
-- Dependencies: 236
-- Data for Name: ModuleMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ModuleMaster" ("ModuleId", "ModuleName") FROM stdin;
1	Dashboard
2	User Master
3	Role Master
4	Visitor Master
5	Department Master
6	Employee Master
7	Appointment Master
8	Visit Entry
10	Reports
11	Entry Requests
\.


--
-- TOC entry 5166 (class 0 OID 16584)
-- Dependencies: 220
-- Data for Name: RoleMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."RoleMaster" ("RoleId", "RoleName", "Description", "Status", "CreatedDate") FROM stdin;
1	Admin	Administrator with full system access	Active	2026-06-16 19:48:29.600374+05:30
2	Security Guard	Security staff managing visitors	Active	2026-06-16 19:48:29.600374+05:30
3	Employee	Company employee	Active	2026-06-16 19:48:29.600374+05:30
5	Receptionist	take appointments 	Active	2026-06-17 10:38:03.347223+05:30
\.


--
-- TOC entry 5184 (class 0 OID 24588)
-- Dependencies: 238
-- Data for Name: RoleRights; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."RoleRights" ("RoleRightId", "RoleId", "ModuleId", "CanView", "CanAdd", "CanEdit", "CanDelete") FROM stdin;
1	1	1	t	t	t	t
2	1	2	t	t	t	t
3	1	3	t	t	t	t
4	1	4	t	t	t	t
5	1	5	t	t	t	t
6	1	6	t	t	t	t
7	1	7	t	t	t	t
8	1	8	t	t	t	t
10	1	10	t	t	t	t
11	2	1	t	f	f	f
12	2	2	f	f	f	f
13	2	3	f	f	f	f
17	2	7	t	f	f	f
18	2	8	t	t	t	f
21	3	1	t	f	f	f
22	3	2	f	f	f	f
23	3	3	f	f	f	f
25	3	5	f	f	f	f
26	3	6	f	f	f	f
28	3	8	f	f	f	f
31	5	1	t	f	f	f
32	5	2	f	f	f	f
33	5	3	f	f	f	f
34	5	4	t	t	t	t
35	5	5	f	f	f	f
36	5	6	t	t	t	t
37	5	7	t	t	t	t
38	5	8	f	f	f	f
40	5	10	t	f	f	f
43	1	11	t	t	t	t
20	2	10	t	f	f	f
47	5	11	f	f	f	f
15	2	5	f	f	f	f
16	2	6	f	f	f	f
14	2	4	t	t	t	f
44	2	11	t	t	t	f
24	3	4	f	f	f	f
30	3	10	t	f	f	f
27	3	7	t	f	t	f
45	3	11	t	f	t	f
\.


--
-- TOC entry 5168 (class 0 OID 16597)
-- Dependencies: 222
-- Data for Name: UserMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."UserMaster" ("UserId", "Username", "FullName", "Email", "MobileNumber", "Password", "RoleId", "Status", "CreatedDate", "LastUpdatedDate", "EmployeeId") FROM stdin;
1	admin	System Administrator	admin@vms.com	9876543210	admin123	1	Active	2026-06-16 19:48:29.612738+05:30	\N	\N
6	rudra123	Rudra Singh	rudra123@gmail.com	9876543211	rudra123	3	Active	2026-06-17 16:17:40.042437+05:30	\N	5
7	rohit123	rohit sharma	rohit123@gmail.com	1234567789	rohit123	3	Active	2026-06-18 10:52:57.409804+05:30	\N	2
3	aarav	Aarav Sharma	aarav123@gmail.com	7634875584	aarav123	2	Active	2026-06-17 12:50:56.817882+05:30	2026-06-18 14:49:23.387597+05:30	\N
8	nakul67	Nakul Chaudhary	nakulchaudhary1234@gmail.com	6573894782	nakul6767	3	Active	2026-06-18 15:35:20.720081+05:30	\N	4
9	garv123	Garv Kumar	garvk123@gmail.com	9876234563	garv123	5	Active	2026-06-19 10:19:43.051476+05:30	\N	\N
\.


--
-- TOC entry 5178 (class 0 OID 16769)
-- Dependencies: 232
-- Data for Name: VisitEntryMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."VisitEntryMaster" ("VisitEntryId", "AppointmentId", "VisitorId", "EmployeeId", "DepartmentId", "CheckInTime", "CheckOutTime", "VisitStatus", "Remarks", "CreatedDate", "EntryRequestId") FROM stdin;
25	30	6	2	2	2026-06-19 12:57:09.09411+05:30	2026-06-19 13:02:19.803089+05:30	Checked Out	\N	2026-06-19 12:41:57.600478+05:30	\N
26	\N	7	4	3	2026-06-19 13:09:26.660747+05:30	2026-06-19 13:10:12.568967+05:30	Checked Out	\N	2026-06-19 13:07:40.81976+05:30	23
27	31	6	2	2	2026-06-19 16:25:29.033121+05:30	2026-06-19 16:26:03.202114+05:30	Checked Out	\N	2026-06-19 16:25:29.033236+05:30	\N
29	\N	7	4	3	2026-06-19 16:38:43.662498+05:30	2026-06-19 16:38:53.798501+05:30	Checked Out	\N	2026-06-19 16:38:43.662498+05:30	24
30	\N	7	4	3	2026-06-19 16:48:02.79384+05:30	2026-06-19 17:18:26.591562+05:30	Checked Out	\N	2026-06-19 16:48:02.793919+05:30	25
31	32	6	2	2	2026-06-19 17:28:38.951752+05:30	2026-06-19 17:28:56.809065+05:30	Checked Out	\N	2026-06-19 17:28:38.951796+05:30	\N
32	\N	7	2	2	2026-06-19 17:44:04.072282+05:30	2026-06-19 17:44:21.529643+05:30	Checked Out	\N	2026-06-19 17:44:04.072283+05:30	26
33	\N	7	4	3	2026-06-19 17:49:40.727605+05:30	2026-06-19 17:49:54.663826+05:30	Checked Out	\N	2026-06-19 17:49:40.727736+05:30	27
34	\N	6	3	5	2026-06-19 17:54:11.466173+05:30	2026-06-19 17:54:17.694089+05:30	Checked Out	\N	2026-06-19 17:54:11.466221+05:30	28
35	33	6	5	4	2026-06-23 12:09:48.031168+05:30	2026-06-23 12:12:13.910605+05:30	Checked Out	\N	2026-06-23 12:09:48.031224+05:30	\N
36	\N	6	5	4	2026-06-23 12:26:00.418041+05:30	2026-06-23 12:26:51.322569+05:30	Checked Out	\N	2026-06-23 12:26:00.418167+05:30	29
\.


--
-- TOC entry 5170 (class 0 OID 16622)
-- Dependencies: 224
-- Data for Name: VisitorMaster; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."VisitorMaster" ("VisitorId", "FirstName", "LastName", "Gender", "MobileNumber", "Email", "Address", "City", "State", "CompanyName", "PhotoPath", "IdProofType", "IdProofNumber", "Status", "CreatedDate", "LastUpdatedDate") FROM stdin;
7	Sparsh	Sharma	Male	3546363985	sparsh123@gmail.com	street 67	udaipur	rajasthan	\N	/uploads/visitors/visitor_7_639174683807068145.jpg	Aadhaar Card	46544574577687	Active	2026-06-19 13:06:58.962943+05:30	\N
6	Harsh	Singh	Male	9876234563	harsh123@gmail.com	street 5 	delhi	delhi	\N	/uploads/visitors/visitor_6_639174686514497544.jpg	Aadhaar Card	352343454366	Active	2026-06-19 12:40:40.652267+05:30	2026-06-23 11:56:53.007984+05:30
\.


--
-- TOC entry 5187 (class 0 OID 24668)
-- Dependencies: 241
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260617103055_AddEmployeeIdToUser	9.0.0
\.


--
-- TOC entry 5204 (class 0 OID 0)
-- Dependencies: 229
-- Name: AppointmentMaster_AppointmentId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."AppointmentMaster_AppointmentId_seq"', 33, true);


--
-- TOC entry 5205 (class 0 OID 0)
-- Dependencies: 227
-- Name: DepartmentMaster_DepartmentId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."DepartmentMaster_DepartmentId_seq"', 7, true);


--
-- TOC entry 5206 (class 0 OID 0)
-- Dependencies: 225
-- Name: EmployeeMaster_EmployeeId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."EmployeeMaster_EmployeeId_seq"', 5, true);


--
-- TOC entry 5207 (class 0 OID 0)
-- Dependencies: 239
-- Name: EntryRequestMaster_EntryRequestId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."EntryRequestMaster_EntryRequestId_seq"', 29, true);


--
-- TOC entry 5208 (class 0 OID 0)
-- Dependencies: 233
-- Name: GatePassMaster_GatePassId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."GatePassMaster_GatePassId_seq"', 33, true);


--
-- TOC entry 5209 (class 0 OID 0)
-- Dependencies: 235
-- Name: ModuleMaster_ModuleId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ModuleMaster_ModuleId_seq"', 11, true);


--
-- TOC entry 5210 (class 0 OID 0)
-- Dependencies: 219
-- Name: RoleMaster_RoleId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."RoleMaster_RoleId_seq"', 6, true);


--
-- TOC entry 5211 (class 0 OID 0)
-- Dependencies: 237
-- Name: RoleRights_RoleRightId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."RoleRights_RoleRightId_seq"', 47, true);


--
-- TOC entry 5212 (class 0 OID 0)
-- Dependencies: 221
-- Name: UserMaster_UserId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."UserMaster_UserId_seq"', 9, true);


--
-- TOC entry 5213 (class 0 OID 0)
-- Dependencies: 231
-- Name: VisitEntryMaster_VisitEntryId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."VisitEntryMaster_VisitEntryId_seq"', 36, true);


--
-- TOC entry 5214 (class 0 OID 0)
-- Dependencies: 223
-- Name: VisitorMaster_VisitorId_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."VisitorMaster_VisitorId_seq"', 7, true);


--
-- TOC entry 4974 (class 2606 OID 16752)
-- Name: AppointmentMaster AppointmentMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AppointmentMaster"
    ADD CONSTRAINT "AppointmentMaster_pkey" PRIMARY KEY ("AppointmentId");


--
-- TOC entry 4970 (class 2606 OID 16725)
-- Name: DepartmentMaster DepartmentMaster_DepartmentName_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DepartmentMaster"
    ADD CONSTRAINT "DepartmentMaster_DepartmentName_key" UNIQUE ("DepartmentName");


--
-- TOC entry 4972 (class 2606 OID 16723)
-- Name: DepartmentMaster DepartmentMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DepartmentMaster"
    ADD CONSTRAINT "DepartmentMaster_pkey" PRIMARY KEY ("DepartmentId");


--
-- TOC entry 4962 (class 2606 OID 16707)
-- Name: EmployeeMaster EmployeeMaster_Email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EmployeeMaster"
    ADD CONSTRAINT "EmployeeMaster_Email_key" UNIQUE ("Email");


--
-- TOC entry 4964 (class 2606 OID 16705)
-- Name: EmployeeMaster EmployeeMaster_EmployeeCode_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EmployeeMaster"
    ADD CONSTRAINT "EmployeeMaster_EmployeeCode_key" UNIQUE ("EmployeeCode");


--
-- TOC entry 4966 (class 2606 OID 16709)
-- Name: EmployeeMaster EmployeeMaster_MobileNumber_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EmployeeMaster"
    ADD CONSTRAINT "EmployeeMaster_MobileNumber_key" UNIQUE ("MobileNumber");


--
-- TOC entry 4968 (class 2606 OID 16703)
-- Name: EmployeeMaster EmployeeMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EmployeeMaster"
    ADD CONSTRAINT "EmployeeMaster_pkey" PRIMARY KEY ("EmployeeId");


--
-- TOC entry 4992 (class 2606 OID 24637)
-- Name: EntryRequestMaster EntryRequestMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EntryRequestMaster"
    ADD CONSTRAINT "EntryRequestMaster_pkey" PRIMARY KEY ("EntryRequestId");


--
-- TOC entry 4978 (class 2606 OID 16828)
-- Name: GatePassMaster GatePassMaster_GatePassNumber_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GatePassMaster"
    ADD CONSTRAINT "GatePassMaster_GatePassNumber_key" UNIQUE ("GatePassNumber");


--
-- TOC entry 4980 (class 2606 OID 16830)
-- Name: GatePassMaster GatePassMaster_VisitEntryId_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GatePassMaster"
    ADD CONSTRAINT "GatePassMaster_VisitEntryId_key" UNIQUE ("VisitEntryId");


--
-- TOC entry 4982 (class 2606 OID 16826)
-- Name: GatePassMaster GatePassMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GatePassMaster"
    ADD CONSTRAINT "GatePassMaster_pkey" PRIMARY KEY ("GatePassId");


--
-- TOC entry 4984 (class 2606 OID 24586)
-- Name: ModuleMaster ModuleMaster_ModuleName_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ModuleMaster"
    ADD CONSTRAINT "ModuleMaster_ModuleName_key" UNIQUE ("ModuleName");


--
-- TOC entry 4986 (class 2606 OID 24584)
-- Name: ModuleMaster ModuleMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ModuleMaster"
    ADD CONSTRAINT "ModuleMaster_pkey" PRIMARY KEY ("ModuleId");


--
-- TOC entry 4994 (class 2606 OID 24674)
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- TOC entry 4943 (class 2606 OID 16595)
-- Name: RoleMaster RoleMaster_RoleName_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RoleMaster"
    ADD CONSTRAINT "RoleMaster_RoleName_key" UNIQUE ("RoleName");


--
-- TOC entry 4945 (class 2606 OID 16593)
-- Name: RoleMaster RoleMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RoleMaster"
    ADD CONSTRAINT "RoleMaster_pkey" PRIMARY KEY ("RoleId");


--
-- TOC entry 4988 (class 2606 OID 24604)
-- Name: RoleRights RoleRights_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RoleRights"
    ADD CONSTRAINT "RoleRights_pkey" PRIMARY KEY ("RoleRightId");


--
-- TOC entry 4990 (class 2606 OID 24606)
-- Name: RoleRights UQ_RoleRights_Role_Module; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RoleRights"
    ADD CONSTRAINT "UQ_RoleRights_Role_Module" UNIQUE ("RoleId", "ModuleId");


--
-- TOC entry 4948 (class 2606 OID 16615)
-- Name: UserMaster UserMaster_Email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserMaster"
    ADD CONSTRAINT "UserMaster_Email_key" UNIQUE ("Email");


--
-- TOC entry 4950 (class 2606 OID 16613)
-- Name: UserMaster UserMaster_Username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserMaster"
    ADD CONSTRAINT "UserMaster_Username_key" UNIQUE ("Username");


--
-- TOC entry 4952 (class 2606 OID 16611)
-- Name: UserMaster UserMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserMaster"
    ADD CONSTRAINT "UserMaster_pkey" PRIMARY KEY ("UserId");


--
-- TOC entry 4976 (class 2606 OID 16786)
-- Name: VisitEntryMaster VisitEntryMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitEntryMaster"
    ADD CONSTRAINT "VisitEntryMaster_pkey" PRIMARY KEY ("VisitEntryId");


--
-- TOC entry 4954 (class 2606 OID 16642)
-- Name: VisitorMaster VisitorMaster_Email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitorMaster"
    ADD CONSTRAINT "VisitorMaster_Email_key" UNIQUE ("Email");


--
-- TOC entry 4956 (class 2606 OID 16644)
-- Name: VisitorMaster VisitorMaster_IdProofNumber_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitorMaster"
    ADD CONSTRAINT "VisitorMaster_IdProofNumber_key" UNIQUE ("IdProofNumber");


--
-- TOC entry 4958 (class 2606 OID 16640)
-- Name: VisitorMaster VisitorMaster_MobileNumber_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitorMaster"
    ADD CONSTRAINT "VisitorMaster_MobileNumber_key" UNIQUE ("MobileNumber");


--
-- TOC entry 4960 (class 2606 OID 16638)
-- Name: VisitorMaster VisitorMaster_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitorMaster"
    ADD CONSTRAINT "VisitorMaster_pkey" PRIMARY KEY ("VisitorId");


--
-- TOC entry 4946 (class 1259 OID 24675)
-- Name: IX_UserMaster_EmployeeId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_UserMaster_EmployeeId" ON public."UserMaster" USING btree ("EmployeeId");


--
-- TOC entry 4998 (class 2606 OID 16758)
-- Name: AppointmentMaster FK_AppointmentMaster_DepartmentMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AppointmentMaster"
    ADD CONSTRAINT "FK_AppointmentMaster_DepartmentMaster" FOREIGN KEY ("DepartmentId") REFERENCES public."DepartmentMaster"("DepartmentId") ON DELETE RESTRICT;


--
-- TOC entry 4999 (class 2606 OID 16763)
-- Name: AppointmentMaster FK_AppointmentMaster_EmployeeMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AppointmentMaster"
    ADD CONSTRAINT "FK_AppointmentMaster_EmployeeMaster" FOREIGN KEY ("EmployeeId") REFERENCES public."EmployeeMaster"("EmployeeId") ON DELETE RESTRICT;


--
-- TOC entry 5000 (class 2606 OID 16753)
-- Name: AppointmentMaster FK_AppointmentMaster_VisitorMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AppointmentMaster"
    ADD CONSTRAINT "FK_AppointmentMaster_VisitorMaster" FOREIGN KEY ("VisitorId") REFERENCES public."VisitorMaster"("VisitorId") ON DELETE RESTRICT;


--
-- TOC entry 4997 (class 2606 OID 16728)
-- Name: EmployeeMaster FK_EmployeeMaster_DepartmentMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EmployeeMaster"
    ADD CONSTRAINT "FK_EmployeeMaster_DepartmentMaster" FOREIGN KEY ("DepartmentId") REFERENCES public."DepartmentMaster"("DepartmentId") ON DELETE RESTRICT;


--
-- TOC entry 5013 (class 2606 OID 24653)
-- Name: EntryRequestMaster FK_EntryRequestMaster_ApprovedByEmployee; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EntryRequestMaster"
    ADD CONSTRAINT "FK_EntryRequestMaster_ApprovedByEmployee" FOREIGN KEY ("ApprovedByEmployeeId") REFERENCES public."EmployeeMaster"("EmployeeId") ON DELETE RESTRICT;


--
-- TOC entry 5014 (class 2606 OID 24643)
-- Name: EntryRequestMaster FK_EntryRequestMaster_DepartmentMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EntryRequestMaster"
    ADD CONSTRAINT "FK_EntryRequestMaster_DepartmentMaster" FOREIGN KEY ("DepartmentId") REFERENCES public."DepartmentMaster"("DepartmentId") ON DELETE RESTRICT;


--
-- TOC entry 5015 (class 2606 OID 24648)
-- Name: EntryRequestMaster FK_EntryRequestMaster_EmployeeMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EntryRequestMaster"
    ADD CONSTRAINT "FK_EntryRequestMaster_EmployeeMaster" FOREIGN KEY ("EmployeeId") REFERENCES public."EmployeeMaster"("EmployeeId") ON DELETE RESTRICT;


--
-- TOC entry 5016 (class 2606 OID 24658)
-- Name: EntryRequestMaster FK_EntryRequestMaster_UserMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EntryRequestMaster"
    ADD CONSTRAINT "FK_EntryRequestMaster_UserMaster" FOREIGN KEY ("CreatedByUserId") REFERENCES public."UserMaster"("UserId") ON DELETE RESTRICT;


--
-- TOC entry 5017 (class 2606 OID 24638)
-- Name: EntryRequestMaster FK_EntryRequestMaster_VisitorMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."EntryRequestMaster"
    ADD CONSTRAINT "FK_EntryRequestMaster_VisitorMaster" FOREIGN KEY ("VisitorId") REFERENCES public."VisitorMaster"("VisitorId") ON DELETE RESTRICT;


--
-- TOC entry 5006 (class 2606 OID 16846)
-- Name: GatePassMaster FK_GatePassMaster_DepartmentMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GatePassMaster"
    ADD CONSTRAINT "FK_GatePassMaster_DepartmentMaster" FOREIGN KEY ("DepartmentId") REFERENCES public."DepartmentMaster"("DepartmentId") ON DELETE RESTRICT;


--
-- TOC entry 5007 (class 2606 OID 16841)
-- Name: GatePassMaster FK_GatePassMaster_EmployeeMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GatePassMaster"
    ADD CONSTRAINT "FK_GatePassMaster_EmployeeMaster" FOREIGN KEY ("EmployeeId") REFERENCES public."EmployeeMaster"("EmployeeId") ON DELETE RESTRICT;


--
-- TOC entry 5008 (class 2606 OID 24663)
-- Name: GatePassMaster FK_GatePassMaster_EntryRequestMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GatePassMaster"
    ADD CONSTRAINT "FK_GatePassMaster_EntryRequestMaster" FOREIGN KEY ("EntryRequestId") REFERENCES public."EntryRequestMaster"("EntryRequestId") ON DELETE SET NULL;


--
-- TOC entry 5009 (class 2606 OID 16831)
-- Name: GatePassMaster FK_GatePassMaster_VisitEntryMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GatePassMaster"
    ADD CONSTRAINT "FK_GatePassMaster_VisitEntryMaster" FOREIGN KEY ("VisitEntryId") REFERENCES public."VisitEntryMaster"("VisitEntryId") ON DELETE RESTRICT;


--
-- TOC entry 5010 (class 2606 OID 16836)
-- Name: GatePassMaster FK_GatePassMaster_VisitorMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GatePassMaster"
    ADD CONSTRAINT "FK_GatePassMaster_VisitorMaster" FOREIGN KEY ("VisitorId") REFERENCES public."VisitorMaster"("VisitorId") ON DELETE RESTRICT;


--
-- TOC entry 5011 (class 2606 OID 24612)
-- Name: RoleRights FK_RoleRights_ModuleMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RoleRights"
    ADD CONSTRAINT "FK_RoleRights_ModuleMaster" FOREIGN KEY ("ModuleId") REFERENCES public."ModuleMaster"("ModuleId") ON DELETE CASCADE;


--
-- TOC entry 5012 (class 2606 OID 24607)
-- Name: RoleRights FK_RoleRights_RoleMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RoleRights"
    ADD CONSTRAINT "FK_RoleRights_RoleMaster" FOREIGN KEY ("RoleId") REFERENCES public."RoleMaster"("RoleId") ON DELETE CASCADE;


--
-- TOC entry 4995 (class 2606 OID 24676)
-- Name: UserMaster FK_UserMaster_EmployeeMaster_EmployeeId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserMaster"
    ADD CONSTRAINT "FK_UserMaster_EmployeeMaster_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES public."EmployeeMaster"("EmployeeId") ON DELETE RESTRICT;


--
-- TOC entry 4996 (class 2606 OID 16616)
-- Name: UserMaster FK_UserMaster_RoleMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserMaster"
    ADD CONSTRAINT "FK_UserMaster_RoleMaster" FOREIGN KEY ("RoleId") REFERENCES public."RoleMaster"("RoleId");


--
-- TOC entry 5001 (class 2606 OID 16787)
-- Name: VisitEntryMaster FK_VisitEntryMaster_AppointmentMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitEntryMaster"
    ADD CONSTRAINT "FK_VisitEntryMaster_AppointmentMaster" FOREIGN KEY ("AppointmentId") REFERENCES public."AppointmentMaster"("AppointmentId") ON DELETE RESTRICT;


--
-- TOC entry 5002 (class 2606 OID 16802)
-- Name: VisitEntryMaster FK_VisitEntryMaster_DepartmentMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitEntryMaster"
    ADD CONSTRAINT "FK_VisitEntryMaster_DepartmentMaster" FOREIGN KEY ("DepartmentId") REFERENCES public."DepartmentMaster"("DepartmentId") ON DELETE RESTRICT;


--
-- TOC entry 5003 (class 2606 OID 16797)
-- Name: VisitEntryMaster FK_VisitEntryMaster_EmployeeMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitEntryMaster"
    ADD CONSTRAINT "FK_VisitEntryMaster_EmployeeMaster" FOREIGN KEY ("EmployeeId") REFERENCES public."EmployeeMaster"("EmployeeId") ON DELETE RESTRICT;


--
-- TOC entry 5004 (class 2606 OID 24683)
-- Name: VisitEntryMaster FK_VisitEntryMaster_EntryRequestMaster_EntryRequestId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitEntryMaster"
    ADD CONSTRAINT "FK_VisitEntryMaster_EntryRequestMaster_EntryRequestId" FOREIGN KEY ("EntryRequestId") REFERENCES public."EntryRequestMaster"("EntryRequestId") ON DELETE SET NULL;


--
-- TOC entry 5005 (class 2606 OID 16792)
-- Name: VisitEntryMaster FK_VisitEntryMaster_VisitorMaster; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VisitEntryMaster"
    ADD CONSTRAINT "FK_VisitEntryMaster_VisitorMaster" FOREIGN KEY ("VisitorId") REFERENCES public."VisitorMaster"("VisitorId") ON DELETE RESTRICT;


-- Completed on 2026-06-23 13:01:17

--
-- PostgreSQL database dump complete
--

\unrestrict zAzPEyvK6pD4biTK2cPUJKfbpAxRskacYEY7b80IIl5RcjTwQC2RZTeXRESURqU


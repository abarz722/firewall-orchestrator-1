/*
Created			29.04.2005
Last modified	13.12.2020
Project			Firewall Orchestrator
Contact			https://cactus.de/fworch
Database		PostgreSQL 9-13
*/

/* Create Sequence 

the abs_hange_id is needed as it is incremented across 4 different tables

*/

Create sequence if not exists "public"."abs_change_id_seq"
Increment 1
Minvalue 1
Maxvalue 9223372036854775807
Cache 1;

-- the device_type table is only needed for the API
-- it allows for the pre-auth functions to work with hasura
Create table "device_type"
(
    "id"      int,
    "name"    VARCHAR
);

-- fundamental firewall data -------------------------------------

Create table "device" -- contains an entry for each firewall gateway
(
	"dev_id" SERIAL,
	"mgm_id" Integer NOT NULL,
	"dev_name" Varchar,
	"local_rulebase_name" Varchar,
	"local_rulebase_uid" Varchar,
	"global_rulebase_name" Varchar,
	"global_rulebase_uid" Varchar,
	"package_name" Varchar,
	"package_uid" Varchar,
	"dev_typ_id" Integer NOT NULL,
	"tenant_id" Integer,
	"dev_active" Boolean NOT NULL Default true,
	"dev_comment" Text,
	"dev_create" Timestamp NOT NULL Default now(),
	"dev_update" Timestamp NOT NULL Default now(),
	"do_not_import" Boolean NOT NULL Default FALSE,
	"clearing_import_ran" Boolean NOT NULL Default FALSE,
	"force_initial_import" Boolean NOT NULL Default FALSE,
	"hide_in_gui" Boolean NOT NULL Default false,
 primary key ("dev_id")
);

Create table "management" -- contains an entry for each firewall management system
(
	"mgm_id" SERIAL,
	"dev_typ_id" Integer NOT NULL,
	"mgm_name" Varchar NOT NULL,
	"mgm_comment" Text,
	"tenant_id" Integer,
	"mgm_create" Timestamp NOT NULL Default now(),
	"mgm_update" Timestamp NOT NULL Default now(),
	"ssh_public_key" Text,
	"ssh_private_key" Text NOT NULL,
	"ssh_hostname" Varchar NOT NULL,
	"ssh_port" Integer NOT NULL Default 22,
	"ssh_user" Varchar NOT NULL Default 'fworch',
	"last_import_md5_complete_config" Varchar Default 0,
	"last_import_md5_rules" Varchar Default 0,
	"last_import_md5_objects" Varchar Default 0,
	"last_import_md5_users" Varchar Default 0,
	"do_not_import" Boolean NOT NULL Default FALSE,
	"clearing_import_ran" Boolean NOT NULL Default false,
	"force_initial_import" Boolean NOT NULL Default FALSE,
	"config_path" Varchar,
	"hide_in_gui" Boolean NOT NULL Default false,
	"importer_hostname" Varchar,
	"debug_level" Integer,
 primary key ("mgm_id")
);

Create table "object"
(
	"obj_id" BIGSERIAL,
	"last_change_admin" Integer,
	"zone_id" Integer,
	"mgm_id" Integer NOT NULL,
	"obj_name" Varchar,
	"obj_comment" Varchar,
	"obj_uid" Text,
	"obj_typ_id" Integer NOT NULL,
	"obj_location" Varchar,
	"obj_member_names" Text,
	"obj_member_refs" Text,
	"initial_config" Boolean NOT NULL Default FALSE,
	"obj_sw" Varchar,
	"obj_ip" Cidr,
	"obj_ip_end" Cidr,
	"obj_nat" Boolean Default false,
	"nattyp_id" Integer,
	"obj_nat_ip" Cidr,
	"obj_nat_ip_end" Cidr,
	"obj_nat_install" Integer,
	"obj_color_id" Integer Default 1,
	"obj_sys_name" Varchar,
	"obj_sys_location" Varchar,
	"obj_sys_contact" Varchar,
	"obj_sys_desc" Text,
	"obj_sys_readcom" Varchar,
	"obj_sys_writecom" Varchar,
	"active" Boolean NOT NULL Default TRUE,
	"obj_create" BIGINT NOT NULL,
	"obj_last_seen" BIGINT NOT NULL,
 primary key ("obj_id")
);

Create table "objgrp"
(
	"objgrp_id" BIGINT NOT NULL,
	"objgrp_member_id" BIGINT NOT NULL,
	"import_created" BIGINT NOT NULL,
	"import_last_seen" BIGINT NOT NULL,
	"active" Boolean NOT NULL Default TRUE,
	"negated" Boolean NOT NULL Default FALSE,
 primary key ("objgrp_id","objgrp_member_id")
);

Create table "rule"
(
	"rule_id" BIGSERIAL,
	"last_change_admin" Integer,
	"rule_name" Varchar,
	"mgm_id" Integer NOT NULL,
	"parent_rule_id" BIGINT,
	"parent_rule_type" smallint,
	"active" Boolean NOT NULL Default TRUE,
	"rule_num" Integer NOT NULL,
	"rule_num_numeric" NUMERIC(16, 8),
	"rule_ruleid" Varchar,
	"rule_uid" Text,
	"rule_disabled" Boolean NOT NULL Default false,
	"rule_src_neg" Boolean NOT NULL Default false,
	"rule_dst_neg" Boolean NOT NULL Default false,
	"rule_svc_neg" Boolean NOT NULL Default false,
	"action_id" Integer NOT NULL,
	"track_id" Integer NOT NULL,
	"rule_src" Text NOT NULL,
	"rule_dst" Text NOT NULL,
	"rule_svc" Text NOT NULL,
	"rule_src_refs" Text,
	"rule_dst_refs" Text,
	"rule_svc_refs" Text,
	"rule_from_zone" Integer,
	"rule_to_zone" Integer,
	"rule_action" Text NOT NULL,
	"rule_track" Text NOT NULL,
	"rule_installon" Varchar,
	"rule_time" Varchar,
	"rule_comment" Text,
	"rule_head_text" Text,
	"rule_implied" Boolean NOT NULL Default FALSE,
	"rule_create" BIGINT NOT NULL,
	"rule_last_seen" BIGINT NOT NULL,
	"dev_id" Integer,
	"access_rule" BOOLEAN Default TRUE,
	"nat_rule" BOOLEAN Default FALSE,
	"xlate_rule" BIGINT,
 primary key ("rule_id")
);

-- rule_metadata contains rule related data that does not change when the rule itself is changed
Create table "rule_metadata"
(
	"rule_metadata_id" BIGSERIAL,
	"dev_id" Integer NOT NULL,
	"rule_uid" Text NOT NULL,
	"rule_created" Timestamp NOT NULL Default now(),
	"rule_last_modified" Timestamp NOT NULL Default now(),
	"rule_first_hit" Timestamp,
	"rule_last_hit" Timestamp,
	"rule_hit_counter" BIGINT,
	"rule_last_certified" Timestamp,
	"rule_last_certifier" Integer,
	"rule_last_certifier_dn" VARCHAR,
	"rule_owner" Integer,
	"rule_owner_dn" Varchar, -- distinguished name pointing to ldap group, path or user
	"rule_to_be_removed" Boolean NOT NULL Default FALSE,
	"last_change_admin" Integer,
	"rule_decert_date" Timestamp,
	"rule_recertification_comment" Varchar,
 primary key ("rule_metadata_id")
);

-- adding direct link tables rule_[svc|nwobj|user]_resolved to make report object export easier
Create table "rule_svc_resolved"
(
	"mgm_id" INT,
	"rule_id" BIGINT NOT NULL,
	"svc_id" BIGINT NOT NULL,
	"created" BIGINT NOT NULL,
	"removed" BIGINT,
 primary key ("mgm_id","rule_id","svc_id")
);

Create table "rule_nwobj_resolved"
(
	"mgm_id" INT,
	"rule_id" BIGINT NOT NULL,
	"obj_id" BIGINT NOT NULL,
	"created" BIGINT NOT NULL,
	"removed" BIGINT,
 primary key ("mgm_id","rule_id","obj_id")
);

Create table "rule_user_resolved"
(
	"mgm_id" INT,
	"rule_id" BIGINT NOT NULL,
	"user_id" BIGINT NOT NULL,
	"created" BIGINT NOT NULL,
	"removed" BIGINT,
 primary key ("mgm_id","rule_id","user_id")
);

Create table "rule_from"
(
	"rule_from_id" BIGSERIAL,
	"rf_create" BIGINT NOT NULL,
	"rf_last_seen" BIGINT NOT NULL,
	"rule_id" BIGINT NOT NULL,
	"obj_id" BIGINT NOT NULL,
	"user_id" BIGINT,
	"active" Boolean NOT NULL Default TRUE,
	"negated" Boolean NOT NULL Default FALSE,
 primary key ("rule_from_id")
);


-- reference rule_metatdata intead?
Create table "rule_review"
(
	"rule_metadata_id" BIGINT NOT NULL,
	"tenant_id" Integer NOT NULL,
	"rr_comment" Text,
	"rr_approved" Boolean NOT NULL Default true,
	"rr_visible" Boolean NOT NULL Default true,
	"rr_create" Timestamp NOT NULL Default now(),
	"rr_update" Timestamp NOT NULL Default now(),
 primary key ("rule_metadata_id","tenant_id")
);

Create table "rule_service"
(
	"rule_id" BIGINT NOT NULL,
	"svc_id" BIGINT NOT NULL,
	"active" Boolean NOT NULL Default TRUE,
	"rs_create" BIGINT NOT NULL,
	"rs_last_seen" BIGINT NOT NULL,
	"negated" Boolean NOT NULL Default FALSE,
 primary key ("rule_id","svc_id")
);

Create table "rule_to"
(
	"rule_id" BIGINT NOT NULL,
	"obj_id" BIGINT NOT NULL,
	"rt_create" BIGINT NOT NULL,
	"rt_last_seen" BIGINT NOT NULL,
	"active" Boolean NOT NULL Default TRUE,
	"negated" Boolean NOT NULL Default FALSE,
 primary key ("rule_id","obj_id")
);

Create table "service"
(
	"svc_id" BIGSERIAL,
	"svc_uid" Text,
	"svc_name" Varchar,
	"svc_typ_id" Integer NOT NULL,
	"mgm_id" Integer,
	"svc_comment" Text,
	"svc_prod_specific" Text,
	"svc_member_names" Text,
	"svc_member_refs" Text,
	"svc_color_id" Integer Default 1,
	"ip_proto_id" Integer,
	"svc_port" Integer Check (svc_port is NULL OR (svc_port>=0 and svc_port<=65535)),
	"svc_port_end" Integer Check (svc_port_end IS NULL OR (svc_port_end>=0 and svc_port_end<=65535)),
	"initial_config" Boolean NOT NULL Default FALSE,
	"srv_keeponinstall" Boolean Default false,
	"svc_rpcnr" Varchar,
	"svc_code" Varchar,
	"svc_match" Text,
	"svc_source_port" Integer Check (svc_source_port is NULL OR (svc_source_port>=0 and svc_source_port<=65535)),
	"svc_source_port_end" Integer Check (svc_source_port_end is NULL OR (svc_source_port_end>=0 and svc_source_port_end<=65535)),
	"svc_tcp_res" Boolean Default false,
	"svc_accept_rep" Boolean Default false,
	"svc_accept_rep_any" Boolean Default false,
	"svc_mfa" Boolean Default false,
	"svc_timeout_std" Boolean Default false,
	"svc_timeout" Integer,
	"svc_sync" Boolean Default false,
	"svc_sync_delay" Boolean Default false,
	"svc_sync_delay_start" Integer,
	"active" Boolean NOT NULL Default TRUE,
	"last_change_admin" Integer,
	"svc_create" BIGINT NOT NULL,
	"svc_last_seen" BIGINT NOT NULL,
 primary key ("svc_id")
);

Create table "svcgrp"
(
	"svcgrp_id" BIGINT NOT NULL,
	"svcgrp_member_id" BIGINT NOT NULL,
	"import_created" BIGINT NOT NULL,
	"import_last_seen" BIGINT NOT NULL,
	"active" Boolean NOT NULL Default TRUE,
	"negated" Boolean NOT NULL Default FALSE,
 primary key ("svcgrp_id","svcgrp_member_id")
);

Create table "zone"
(
	"zone_id" SERIAL,
	"zone_create" BIGINT NOT NULL,
	"zone_last_seen" BIGINT NOT NULL,
	"mgm_id" Integer NOT NULL,
	"zone_name" Varchar NOT NULL,
	"active" Boolean NOT NULL Default TRUE,
 primary key ("zone_id")
);

Create table "usr"
(
	"user_id" BIGSERIAL PRIMARY KEY,
	"usr_typ_id" Integer NOT NULL,
	"user_color_id" Integer Default 1,
	"mgm_id" Integer NOT NULL,
	"user_name" Varchar NOT NULL,
	"active" Boolean NOT NULL Default TRUE,
	"user_member_names" Text,
	"user_member_refs" Text,
	"user_authmethod" Varchar,
	"user_valid_from" Date Default '1900-01-01',
	"user_valid_until" Date Default '9999-12-31',
	"src_restrict" Text,
	"dst_restrict" Text,
	"time_restrict" Text,
	"user_create" BIGINT NOT NULL,
	"user_last_seen" BIGINT NOT NULL,
	"user_comment" Text,
	"user_uid" Text,
	"user_firstname" Varchar,
	"user_lastname" Varchar,
	"last_change_admin" Integer,
	"tenant_id" Integer
);

Create table "usergrp"
(
	"usergrp_id" BIGINT,
	"usergrp_member_id" BIGINT,
	"import_created" BIGINT NOT NULL,
	"import_last_seen" BIGINT NOT NULL,
	"active" Boolean NOT NULL Default TRUE,
 primary key ("usergrp_id","usergrp_member_id")
);

Create table "usergrp_flat"
(
	"active" Boolean NOT NULL Default TRUE,
	"usergrp_flat_id" BIGINT NOT NULL,
	"usergrp_flat_member_id" BIGINT NOT NULL,
	"import_created" BIGINT NOT NULL,
	"import_last_seen" BIGINT NOT NULL,
 primary key ("usergrp_flat_id","usergrp_flat_member_id")
);

Create table "objgrp_flat"
(
	"objgrp_flat_id" BIGINT NOT NULL,
	"objgrp_flat_member_id" BIGINT NOT NULL,
	"active" Boolean NOT NULL Default TRUE,
	"import_created" BIGINT NOT NULL,
	"import_last_seen" BIGINT NOT NULL,
	"negated" Boolean NOT NULL Default FALSE
);

Create table "svcgrp_flat"
(
	"svcgrp_flat_id" Integer NOT NULL,
	"svcgrp_flat_member_id" Integer NOT NULL,
	"import_created" Integer NOT NULL,
	"import_last_seen" Integer NOT NULL,
	"active" Boolean NOT NULL Default TRUE,
	"negated" Boolean NOT NULL Default FALSE
);

-- uiuser - change metadata -------------------------------------

Create table "uiuser"
(
	"uiuser_id" SERIAL NOT NULL,
	"uiuser_username" Varchar,
	"uuid" Varchar NOT NULL UNIQUE,
	"uiuser_first_name" Varchar,
	"uiuser_last_name" Varchar,
	"uiuser_start_date" Date Default now(),
	"uiuser_end_date" Date,
	"uiuser_email" Varchar,
	"tenant_id" Integer,
	"uiuser_language" Varchar,
	"uiuser_password_must_be_changed" Boolean NOT NULL Default TRUE,
	"uiuser_last_login" Timestamp with time zone,
	"uiuser_last_password_change" Timestamp with time zone,
	"uiuser_pwd_history" Text,
	"ldap_connection_id" BIGINT,
 primary key ("uiuser_id")
);

-- text tables ----------------------------------------

Create table "language"
(
	"name" Varchar NOT NULL UNIQUE,
	"culture_info" Varchar NOT NULL,
	primary key ("name")
);

Create table "txt"
(
	"id" Varchar NOT NULL,
	"language" Varchar NOT NULL,
	"txt" Varchar NOT NULL,
 primary key ("id", "language")
);

Create table "error"
(
	"error_id" Varchar NOT NULL UNIQUE,
	"error_lvl" Integer NOT NULL,
	"error_txt_ger" Text NOT NULL,
	"error_txt_eng" Text NOT NULL,
 primary key ("error_id")
);

Create table "error_log"
(
	"error_log_id" BIGSERIAL,
	"error_id" Varchar NOT NULL,
	"error_txt" Text,
	"error_time" Timestamp NOT NULL Default now(),
 primary key ("error_log_id")
);

-- tenant -------------------------------------
Create table "tenant"
(
	"tenant_id" SERIAL,
	"tenant_name" Varchar NOT NULL,
	"tenant_projekt" Varchar,
	"tenant_comment" Text,
	"tenant_report" Boolean Default true,
	"tenant_can_view_all_devices" Boolean NOT NULL Default false,
	"tenant_is_superadmin" Boolean NOT NULL default false,	
	"tenant_create" Timestamp NOT NULL Default now(),
 primary key ("tenant_id")
);

Create table "tenant_to_device"
(
	"tenant_id" Integer NOT NULL,
	"device_id" Integer NOT NULL,
 primary key ("tenant_id", "device_id")
);

Create table "tenant_object"
(
	"tenant_id" Integer NOT NULL,
	"obj_id" BIGINT NOT NULL,
 primary key ("tenant_id","obj_id")
);

Create table "tenant_network"
(
	"tenant_net_id" BIGSERIAL,
	"tenant_id" Integer NOT NULL,
	"tenant_net_name" Varchar,
	"tenant_net_comment" Text,
	"tenant_net_ip" Cidr,
	"tenant_net_ip_end" Cidr,
	"tenant_net_create" Timestamp NOT NULL Default now(),
 primary key ("tenant_net_id")
);

-- unused in 5.0, moved to ldap
Create table "tenant_user"
(
	"user_id" BIGSERIAL,
	"tenant_id" BIGSERIAL,
 primary key ("user_id","tenant_id")
);

-- unused in 5.0, moved to ldap
Create table "tenant_username"
(
	"tenant_username_id" BIGSERIAL,
	"tenant_id" Integer,
	"tenant_username_pattern" Varchar,
	"tenant_username_comment" Text,
	"tenant_username_create" Timestamp NOT NULL Default now(),
 primary key ("tenant_username_id")
);

-- basic static data -------------------------------------

Create table "parent_rule_type"
(
	"id" smallserial NOT NULL,
	"name" Varchar NOT NULL,
 primary key ("id")
);

Create table "stm_action"
(
	"action_id" SERIAL,
	"action_name" Varchar NOT NULL,
 primary key ("action_id")
);

Create table "stm_color"
(
	"color_id" SERIAL,
	"color_name" Varchar NOT NULL,
	"color_rgb" Char(7) NOT NULL,
	"color_comment" Text,
 primary key ("color_id")
);

Create table "stm_dev_typ"
(
	"dev_typ_id" SERIAL,
	"dev_typ_manufacturer" Varchar,
	"dev_typ_name" Varchar NOT NULL,
	"dev_typ_version" Varchar NOT NULL,
	"dev_typ_comment" Text,
	"dev_typ_predef_svc" Text,
	"dev_typ_predef_obj" Text,
	"dev_typ_is_mgmt" Boolean,
	"dev_typ_config_file_rules" Varchar,
	"dev_typ_config_file_basic_objects" Varchar,
	"dev_typ_config_file_users" Varchar,
 primary key ("dev_typ_id")
);

Create table "stm_nattyp"
(
	"nattyp_id" SERIAL,
	"nattyp_name" Varchar NOT NULL,
	"nattyp_comment" Text,
 primary key ("nattyp_id")
);

Create table "stm_obj_typ"
(
	"obj_typ_id" SERIAL,
	"obj_typ_name" Varchar NOT NULL,
	"obj_typ_comment" Text,
 primary key ("obj_typ_id")
);

Create table "stm_track"
(
	"track_id" SERIAL,
	"track_name" Varchar NOT NULL,
 primary key ("track_id")
);

Create table "stm_ip_proto"
(
	"ip_proto_id" Integer NOT NULL,
	"ip_proto_name" Varchar,
	"ip_proto_comment" Text,
 primary key ("ip_proto_id")
);

Create table "stm_svc_typ"
(
	"svc_typ_id" Integer NOT NULL UNIQUE,
	"svc_typ_name" Varchar,
	"svc_typ_comment" Text,
 primary key ("svc_typ_id")
);

Create table "stm_usr_typ"
(
	"usr_typ_id" Integer NOT NULL UNIQUE,
	"usr_typ_name" Varchar,
 primary key ("usr_typ_id")
);

-- only permanent import table -----------------------------------------------
-- these tables are only filled during an import run and the import data
-- is immediately removed afterwards

Create table "import_control"
(
	"control_id" BIGSERIAL,
	"start_time" Timestamp NOT NULL Default now(),
	"stop_time" Timestamp,
	"is_initial_import" Boolean NOT NULL Default FALSE,
	"delimiter_group" Varchar(3) NOT NULL Default '|',
	"delimiter_zone" Varchar(3) Default '%',
	"delimiter_user" Varchar(3) Default '@',
	"delimiter_list" Varchar(3) Default '|',
	"mgm_id" Integer NOT NULL,
	"last_change_in_config" Timestamp,
	"successful_import" Boolean NOT NULL Default FALSE,
	"changes_found" Boolean NOT NULL Default FALSE,
	"import_errors" Varchar,
 primary key ("control_id")
);

-- temporary table for storing the fw-relevant config during import
CREATE TABLE IF NOT EXISTS "import_config" (
    "import_id" bigint NOT NULL,
    "mgm_id" integer NOT NULL,
    "config" jsonb NOT NULL,
    PRIMARY KEY ("import_id")
);

-- permanent table for storing the full config as an archive
CREATE TABLE "import_full_config" (
    "import_id" bigint NOT NULL,
    "mgm_id" integer NOT NULL,
    "config" jsonb NOT NULL,
    PRIMARY KEY ("import_id")
);

-- temporary import tables -------------------------------------

Create table "import_service"
(
	"svc_id" BIGSERIAL,
	"control_id" BIGINT NOT NULL,
	"svc_typ" Text NOT NULL,
	"svc_name" Varchar,
	"svc_comment" Text,
	"svc_color" Text Default 'black',
	"ip_proto" Text,
	"svc_prod_specific" Text,
	"rpc_nr" Varchar,
	"svc_uid" Text,
	"svc_port" Integer,
	"svc_port_end" Integer,
	"svc_source_port" Integer,
	"svc_source_port_end" Integer,
	"svc_timeout_std" Boolean Default false,
	"svc_timeout" Integer,
	"svc_member_names" Text,
	"svc_member_refs" Text,
	"last_change_admin" Varchar,
	"last_change_time" Timestamp,
	"svc_scope" Varchar,
 primary key ("svc_id","control_id")
);

Create table "import_object"
(
	"obj_id" BIGSERIAL,
	"obj_zone" Text,
	"obj_name" Varchar,
	"obj_typ" Text NOT NULL,
	"obj_member_names" Text,
	"obj_member_refs" Text,
	"obj_member_excludes" Text,
	"obj_sw" Varchar,
	"obj_ip" Cidr,
	"obj_ip_end" Cidr,
	"obj_color" Text Default 'black',
	"obj_comment" Text,
	"obj_location" Text,
	"control_id" BIGINT NOT NULL,
	"obj_uid" Text,
	"last_change_admin" Varchar,
	"last_change_time" Timestamp,
	"obj_scope" Varchar,
 primary key ("obj_id","control_id")
);

Create table "import_user"
(
	"user_id" BIGSERIAL,
	"control_id" BIGINT NOT NULL,
	"user_color" Text Default 'black',
	"user_name" Varchar NOT NULL,
	"user_typ" Text,
	"user_comment" Text,
	"user_authmethod" Varchar,
	"user_valid_from" Text,
	"user_valid_until" Text,
	"user_member_names" Text,
	"user_member_refs" Text,
	"user_uid" Text,
	"user_firstname" Text,
	"user_lastname" Text,
	"src_restrict" Text,
	"dst_restrict" Text,
	"time_restrict" Text,
	"last_change_admin" Varchar,
	"last_change_time" Timestamp,
	"user_scope" Varchar,
 primary key ("user_id","control_id")
);

Create table "import_rule"
(
	"control_id" BIGINT NOT NULL,
	"rule_id" BIGSERIAL,
	"rulebase_name" Varchar NOT NULL,
	"rule_num" Integer NOT NULL,
	"rule_uid" Text NOT NULL,
	"rule_ruleid" Varchar,
	"rule_name" Varchar,
	"rule_sysid" Varchar,
	"rule_disabled" Boolean Default false,
	"rule_src_neg" Boolean Default false,
	"rule_dst_neg" Boolean Default false,
	"rule_svc_neg" Boolean Default false,
	"rule_implied" Boolean Default FALSE,
	"rule_src" Text NOT NULL,
	"rule_dst" Text NOT NULL,
	"rule_from_zone" Text,
	"rule_to_zone" Text,
	"rule_svc" Text,
	"rule_action" Text NOT NULL,
	"rule_track" Text NOT NULL,
	"rule_installon" Varchar,
	"rule_time" Varchar,
	"rule_comment" Text,
	"rule_head_text" Text,
	"last_change_admin" Varchar,
	"last_change_time" Timestamp,
	"rule_scope" Varchar,
	"rule_src_refs" Text,
	"rule_dst_refs" Text,
	"rule_svc_refs" Text,
	"parent_rule_uid" Text,
	"rule_type" Varchar Default 'access',
 primary key ("control_id","rule_id")
);

Create table "import_zone"
(
	"control_id" BIGINT NOT NULL,
	"zone_name" Text NOT NULL,
	"last_change_time" Timestamp
);

-- changelog tables -------------------------------------

Create table "import_changelog"
(
	"change_time" Timestamp,
	"management_name" Varchar,
	"changed_object_name" Varchar,
	"changed_object_uid" Varchar,
	"changed_object_type" Varchar,
	"change_action" Varchar NOT NULL,
	"change_admin" Varchar,
	"control_id" BIGINT NOT NULL,
	"import_changelog_nr" BIGINT,
	"import_changelog_id" BIGSERIAL,
 primary key ("import_changelog_id")
);

Create table "changelog_object"
(
	"log_obj_id" BIGSERIAL,
	"new_obj_id" BIGINT Constraint "changelog_object_new_obj_id_constraint" Check ((change_action='D' AND new_obj_id IS NULL) OR NOT new_obj_id IS NULL),
	"old_obj_id" BIGINT Constraint "changelog_object_old_obj_id_constraint" Check ((change_action='I' AND old_obj_id IS NULL) OR NOT old_obj_id IS NULL),
	"import_admin" Integer,
	"doku_admin" Integer,
	"control_id" BIGINT NOT NULL,
	"abs_change_id" BIGINT NOT NULL Default nextval('public.abs_change_id_seq'::text) UNIQUE,
	"change_action" Char(1) NOT NULL,
	"changelog_obj_comment" Text,
	"documented" Boolean NOT NULL Default FALSE,
	"docu_time" Timestamp,
	"mgm_id" Integer NOT NULL,
	"change_type_id" Integer NOT NULL Default 3,
	"security_relevant" Boolean NOT NULL Default TRUE,
	"change_request_info" Varchar,
	"change_time" Timestamp,
	"unique_name" Varchar,
 primary key ("log_obj_id")
);

Create table "changelog_service"
(
	"log_svc_id" BIGSERIAL,
	"doku_admin" Integer,
	"control_id" BIGINT NOT NULL,
	"import_admin" Integer,
	"new_svc_id" BIGINT Constraint "changelog_service_new_svc_id_constraint" Check ((change_action='D' AND new_svc_id IS NULL) OR NOT new_svc_id IS NULL),
	"old_svc_id" BIGINT Constraint "changelog_service_old_svc_id_constraint" Check ((change_action='I' AND old_svc_id IS NULL) OR NOT old_svc_id IS NULL),
	"abs_change_id" BIGINT NOT NULL Default nextval('public.abs_change_id_seq'::text) UNIQUE,
	"change_action" Char(1) NOT NULL,
	"changelog_svc_comment" Text,
	"documented" Boolean NOT NULL Default FALSE,
	"docu_time" Timestamp,
	"mgm_id" Integer NOT NULL,
	"change_type_id" Integer NOT NULL Default 3,
	"security_relevant" Boolean NOT NULL Default TRUE,
	"change_request_info" Varchar,
	"change_time" Timestamp,
	"unique_name" Varchar,
 primary key ("log_svc_id")
);

Create table "changelog_user"
(
	"log_usr_id" BIGSERIAL,
	"new_user_id" BIGINT Constraint "changelog_user_new_user_id_constraint" Check ((change_action='D' AND new_user_id IS NULL) OR NOT new_user_id IS NULL),
	"old_user_id" BIGINT Constraint "changelog_user_old_user_id_contraint" Check ((change_action='I' AND old_user_id IS NULL) OR NOT old_user_id IS NULL),
	"import_admin" Integer,
	"doku_admin" Integer,
	"control_id" BIGINT NOT NULL,
	"abs_change_id" BIGINT NOT NULL Default nextval('public.abs_change_id_seq'::text) UNIQUE,
	"change_action" Char(1) NOT NULL,
	"changelog_user_comment" Text,
	"documented" Boolean NOT NULL Default FALSE,
	"docu_time" Timestamp,
	"mgm_id" Integer NOT NULL,
	"change_type_id" Integer NOT NULL Default 3,
	"security_relevant" Boolean NOT NULL Default TRUE,
	"change_request_info" Varchar,
	"change_time" Timestamp,
	"unique_name" Varchar,
 primary key ("log_usr_id")
);

Create table "changelog_rule"
(
	"log_rule_id" BIGSERIAL,
	"doku_admin" Integer,
	"control_id" BIGINT NOT NULL,
	"import_admin" Integer,
	"new_rule_id" BIGINT Constraint "changelog_rule_new_rule_id_constraint" Check ((change_action='D' AND new_rule_id IS NULL) OR NOT new_rule_id IS NULL),
	"old_rule_id" BIGINT Constraint "changelog_rule_old_rule_id_constraint" Check ((change_action='I' AND old_rule_id IS NULL) OR NOT old_rule_id IS NULL),
	"implicit_change" Boolean NOT NULL Default FALSE,
	"abs_change_id" BIGINT NOT NULL Default nextval('public.abs_change_id_seq'::text) UNIQUE,
	"change_action" Char(1) NOT NULL,
	"changelog_rule_comment" Text,
	"documented" Boolean NOT NULL Default FALSE,
	"docu_time" Timestamp,
	"mgm_id" Integer NOT NULL,
	"dev_id" Integer NOT NULL,
	"change_type_id" Integer NOT NULL Default 3,
	"security_relevant" Boolean NOT NULL Default TRUE,
	"change_request_info" Varchar,
	"change_time" Timestamp,
	"unique_name" Varchar,
 primary key ("log_rule_id")
);

-- request handling -------------------------------------------

Create table "request"
(
	"request_id" BIGSERIAL,
	"request_number" Varchar,
	"request_time" Timestamp,
	"request_received" Timestamp,
	"request_submitter" Varchar,
	"request_approver" Varchar,
	"tenant_id" Integer,
	"request_type_id" Integer,
 primary key ("request_id")
);

Create table "request_object_change"
(
	"log_obj_id" BIGINT NOT NULL,
	"request_id" Integer NOT NULL,
 primary key ("log_obj_id","request_id")
);

Create table "request_service_change"
(
	"log_svc_id" BIGINT NOT NULL,
	"request_id" Integer NOT NULL,
 primary key ("log_svc_id","request_id")
);

Create table "request_rule_change"
(
	"log_rule_id" BIGINT NOT NULL,
	"request_id" Integer NOT NULL,
 primary key ("log_rule_id","request_id")
);

Create table "request_user_change"
(
	"log_usr_id" BIGINT NOT NULL,
	"request_id" Integer NOT NULL,
 primary key ("log_usr_id","request_id")
);

Create table "request_type"
(
	"request_type_id" Integer NOT NULL UNIQUE,
	"request_type_name" Varchar NOT NULL UNIQUE,
	"request_type_comment" Varchar,
 primary key ("request_type_id")
);

Create table "stm_change_type"
(
	"change_type_id" SERIAL,
	"change_type_name" Varchar,
 primary key ("change_type_id")
);

-- reporting -------------------------------------------------------

Create table "report_template"
(
	"report_template_id" SERIAL,
	"report_filter" Varchar,
	"report_template_name" Varchar, --  NOT NULL Default "Report_"|"report_id"::VARCHAR,  -- user given name of a report
	"report_template_comment" TEXT,
	"report_template_create" Timestamp DEFAULT now(),
	"report_template_owner" Integer, --FK
	"filterline_history" Boolean Default TRUE, -- every time a filterline is sent, we save it for future usage (auto-deleted every 90 days)
	primary key ("report_template_id")
);

Create table "report_format"
(
	"report_format_name" varchar not null,
 	primary key ("report_format_name")
);

Create table "report_schedule_format"
(
	"report_schedule_format_name" VARCHAR not null,
	"report_schedule_id" BIGSERIAL,
 	primary key ("report_schedule_format_name","report_schedule_id")
);

Create table "report"
(
	"report_id" BIGSERIAL,
	"report_template_id" Integer,
	"report_start_time" Timestamp,
	"report_end_time" Timestamp,
	"report_json" json NOT NULL,
	"report_pdf" text,
	"report_csv" text,
	"report_html" text,
	"report_name" varchar NOT NULL,
	"report_owner_id" Integer NOT NULL, --FK to uiuser
	"tenant_wide_visible" Integer,
 	primary key ("report_id")
);

Create table if not exists "report_schedule"
(
	"report_schedule_id" BIGSERIAL,
	"report_schedule_name" Varchar, --  NOT NULL Default "Report_"|"report_id"::VARCHAR,  -- user given name of a report
	"report_template_id" Integer, --FK
	"report_schedule_owner" Integer NOT NULL, --FK
	"report_schedule_start_time" Timestamp NOT NULL,  -- if day is bigger than 28, simply use the 1st of the next month, 00:00 am
	"report_schedule_repeat" Integer Not NULL Default 0, -- 0 do not repeat, 1 daily, 2 weekly, 3 monthly, 4 yearly 
	"report_schedule_every" Integer Not NULL Default 1, -- x - every x days/weeks/months/years
	"report_schedule_active" Boolean Default TRUE,
	"report_schedule_repetitions" Integer,
 	primary key ("report_schedule_id")
);

Create table "report_template_viewable_by_tenant"
(
	"report_template_id" Integer NOT NULL,
	"tenant_id" Integer NOT NULL,
 	primary key ("tenant_id","report_template_id")
);

Create table "report_template_viewable_by_user"
(
	"report_template_id" Integer NOT NULL,
	"uiuser_id" Integer NOT NULL,
 	primary key ("uiuser_id","report_template_id")
);

-- configuration

Create table "ldap_connection"
(
	"ldap_connection_id" BIGSERIAL,
	"ldap_server" Varchar NOT NULL,
	"ldap_port" Integer NOT NULL Default 636,
	"ldap_tls" Boolean NOT NULL Default TRUE,
	"ldap_searchpath_for_users" Varchar NOT NULL,
	"ldap_searchpath_for_roles" Varchar,
	"ldap_tenant_level" Integer NOT NULL Default 1,
	"ldap_search_user" Varchar NOT NULL,
	"ldap_search_user_pwd" Varchar NOT NULL,
	"ldap_write_user" Varchar,
	"tenant_id" Integer,
	"ldap_write_user_pwd" Varchar,
	"ldap_searchpath_for_groups" Varchar,
	"ldap_type" Integer NOT NULL Default 0,
	"ldap_pattern_length" Integer NOT NULL Default 0,
	"ldap_name" Varchar,
	primary key ("ldap_connection_id")
);

Create table "config"
(
	"config_key" VARCHAR NOT NULL,
	"config_value" VARCHAR,
	"config_user" Integer,
	primary key ("config_key","config_user")
);

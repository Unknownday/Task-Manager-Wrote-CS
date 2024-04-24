CREATE SEQUENCE user_id_seq
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

CREATE SEQUENCE desk_id_seq
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

CREATE SEQUENCE project_id_seq
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

CREATE SEQUENCE task_id_seq
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;


CREATE TABLE IF NOT EXISTS Users (
	user_id INTEGER NOT NULL PRIMARY KEY DEFAULT nextval('user_id_seq'::regclass),
	user_email VARCHAR NOT NULL UNIQUE,
	user_phone VARCHAR UNIQUE,
	user_firstname VARCHAR,
	user_surname VARCHAR,
	user_nickname VARCHAR,
	user_registration_date TIMESTAMP,
	user_last_login_date TIMESTAMP,
	user_profile_photo BYTEA,
	user_profile_description TEXT,
	user_status INTEGER,
	user_refresh_token TEXT
);

CREATE TABLE IF NOT EXISTS Desks (
	desk_id INTEGER NOT NULL PRIMARY KEY DEFAULT nextval('desk_id_seq'::regclass),
	desk_name VARCHAR,
	desk_description TEXT,
	desk_creation_date TIMESTAMP,
	desk_photo BYTEA,
	desk_is_public BOOLEAN,
	desk_administrator_id INTEGER,
	CONSTRAINT desks_desk_administrator_id_fkey FOREIGN KEY (desk_administrator_id)
		REFERENCES Users (user_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION
);

CREATE TABLE IF NOT EXISTS Projects (
	project_id INTEGER NOT NULL PRIMARY KEY DEFAULT nextval('project_id_seq'::regclass),
	project_creator_id INTEGER NOT NULL,
	project_name VARCHAR,
	project_description VARCHAR,
	project_creation_date TIMESTAMP,
	project_photo BYTEA,
	project_status INTEGER,
	CONSTRAINT project_project_creator_id_fkey FOREIGN KEY (project_creator_id)
        REFERENCES Users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Tasks (
	task_id INTEGER NOT NULL PRIMARY KEY DEFAULT nextval('task_id_seq'::regclass),
	task_name VARCHAR,
	task_description TEXT,
	task_creation_date TIMESTAMP,
	task_photo BYTEA,
	task_start_date TIMESTAMP,
	task_end_date TIMESTAMP,
	task_file BYTEA,
	task_column TEXT,
	task_creator_id INTEGER NOT NULL,
	task_executor_id INTEGER NOT NULL,
	CONSTRAINT tasks_task_creator_id_fkey FOREIGN KEY (task_creator_id)
		REFERENCES Users (user_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION,
	CONSTRAINT tasks_task_executor_id_fkey FOREIGN KEY (task_executor_id)
		REFERENCES Users (user_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION
);

CREATE TABLE IF NOT EXISTS ProjectMembers (
	project_id INTEGER NOT NULL,
	user_id INTEGER NOT NULL,
	CONSTRAINT ProjectMembers_user_id_fkey FOREIGN KEY (user_id)
		REFERENCES Users (user_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION,
	CONSTRAINT ProjectMembers_project_id_fkey FOREIGN KEY (project_id)
		REFERENCES Projects (project_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION
);

CREATE TABLE IF NOT EXISTS ProjectDesks (
	project_id INTEGER NOT NULL,
	desk_id INTEGER NOT NULL,
	CONSTRAINT ProjectDesks_desk_id_fkey FOREIGN KEY (desk_id)
		REFERENCES Desks (desk_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION,
	CONSTRAINT ProjectDesks_project_id_fkey FOREIGN KEY (project_id)
		REFERENCES Projects (project_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION
);

CREATE TABLE IF NOT EXISTS DeskColumns (
	desk_column TEXT NOT NULL,
	desk_id INTEGER NOT NULL,
	CONSTRAINT DeskColumns_desk_id_fkey FOREIGN KEY (desk_id)
		REFERENCES Desks (desk_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION
);

CREATE TABLE IF NOT EXISTS DeskTasks (
	task_id INTEGER NOT NULL,
	desk_id INTEGER NOT NULL,
	CONSTRAINT DeskTasks_desk_id_fkey FOREIGN KEY (desk_id)
		REFERENCES Desks (desk_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION,
	CONSTRAINT DeskTasks_task_id_fkey FOREIGN KEY (task_id)
		REFERENCES Tasks (task_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION
);

CREATE TABLE IF NOT EXISTS ProjectStuff (
	project_id INTEGER NOT NULL,
	user_id INTEGER NOT NULL,
	user_status INTEGER NOT NULL,
	CONSTRAINT ProjectStuff_user_id_fkey FOREIGN KEY (user_id)
		REFERENCES Users (user_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION,
	CONSTRAINT ProjectStuff_project_id_fkey FOREIGN KEY (project_id)
		REFERENCES Projects (project_id) MATCH SIMPLE
		ON DELETE CASCADE
		ON UPDATE NO ACTION
);






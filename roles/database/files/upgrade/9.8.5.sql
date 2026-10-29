ALTER TABLE request.reqelement
    ADD COLUMN IF NOT EXISTS time_object_task_id bigint;

ALTER TABLE request.implelement
    ADD COLUMN IF NOT EXISTS time_object_task_id bigint;

ALTER TABLE request.reqelement
    DROP CONSTRAINT IF EXISTS request_reqelement_time_object_task_foreign_key;

ALTER TABLE request.reqelement
    ADD CONSTRAINT request_reqelement_time_object_task_foreign_key
    FOREIGN KEY (time_object_task_id)
    REFERENCES request.reqtask(id)
    ON UPDATE RESTRICT
    ON DELETE SET NULL;

ALTER TABLE request.implelement
    DROP CONSTRAINT IF EXISTS request_implelement_time_object_task_foreign_key;

ALTER TABLE request.implelement
    ADD CONSTRAINT request_implelement_time_object_task_foreign_key
    FOREIGN KEY (time_object_task_id)
    REFERENCES request.reqtask(id)
    ON UPDATE RESTRICT
    ON DELETE SET NULL;

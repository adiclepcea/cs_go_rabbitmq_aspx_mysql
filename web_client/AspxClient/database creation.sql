create database utimeline;

use utimeline;
create table timeline (id integer not null, timepoint timestamp default CURRENT_TIMESTAMP, id_agent integer not null, x integer not null, y integer not null, primary key PK_ID(id));
create index IX_TIMEPOINT on timeline(timepoint,id_agent);
create user 'user_timeline'@'%' identified by 'UserPass123!';

grant select, insert, update, delete on utimeline.* to 'user_timeline'@'%';
create database utimeline;

use utimeline;
create table timeline (id integer auto_increment, timepoint timestamp(3) default CURRENT_TIMESTAMP(3), id_agent integer not null, x integer not null, y integer not null, primary key PK_ID(id));
create index IX_TIMEPOINT on timeline(timepoint,id_agent);
create user 'user_timeline'@'%' identified by 'UserPass123!';

grant select, insert, update, delete on utimeline.* to 'user_timeline'@'%';
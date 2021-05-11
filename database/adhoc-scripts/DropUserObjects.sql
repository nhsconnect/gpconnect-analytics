/*
    Identify all user objects and create drop
*/

select 
    case 
        when type = 'FN' then 'drop function ' + s.name + '.' + o.name + ';'
        when type = 'P' then 'drop procedure ' + s.name + '.' + o.name + ';'
        when type = 'V' then 'drop view ' + s.name + '.' + o.name + ';'
        when type = 'U' then 'drop table ' + s.name + '.' + o.name + ';'
        else '(unknown object type ' + s.name + '.' + o.name + ')'
    end as sql
from sys.objects o
inner join sys.schemas s on o.schema_id = s.schema_id
where o.type != 'S' 
and s.name not in ('sys', 'jobs', 'jobs_internal')
and o.type not in ('C', 'F', 'PK', 'SQ', 'UQ')
order by 
    o.type_desc asc, 
    o.name asc;
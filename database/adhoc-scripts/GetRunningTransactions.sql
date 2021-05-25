/*
    Identify all user objects and create drop
*/

select
    getdate() as now,
    datediff(second, transaction_begin_time, getdate()) as tran_elapsed_time_seconds,
    st.session_id,
    txt.text, 
    *
from sys.dm_tran_active_transactions at
inner join sys.dm_tran_session_transactions st on st.transaction_id = at.transaction_id
left outer join sys.dm_exec_sessions sess on st.session_id = sess.session_id
left outer join sys.dm_exec_connections conn on conn.session_id = sess.session_id
outer apply sys.dm_exec_sql_text(conn.most_recent_sql_handle) as txt
order by tran_elapsed_time_seconds desc;

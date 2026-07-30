#ifndef __RING_BUFF_H__
#define __RING_BUFF_H__

typedef enum {
    RB_ERR_CODE_FULL  = -2,
    RB_ERR_CODE_EMPTY = -3,
    RB_ERR_CODE_HEAD  = -4,
    RB_ERR_CODE_LEAK  = -5,
    RB_ERR_CODE_BUTT  = -256,
} RB_ERR_CODE_EN;

typedef struct _ring_buff_head_t 
{
    NI_S32 len;
    NI_U32 type;
} ring_buff_head_t;

typedef enum niRB_TYPE_EN
{
    RB_TYPE_NONE = 0,
    RB_TYPE_DATA ,
    RB_TYPE_BUTT,
} RB_TYPE_EN;

typedef struct _ring_buffer_t {
    NI_S32 read_head    ;
    NI_S32 read_tail    ;
    NI_S32 write_head   ;
    NI_S32 write_tail   ;
    NI_S32 len          ;
    NI_S32 data_len     ;
	NI_S32 reserv       ;
    NI_BOOL is_alloc    ;
    NI_VOID* pbuff      ;
    NI_S32 blk_len      ;
#if __linux__
    pthread_mutex_t psection;
#endif
} ring_buffer_t;

ring_buffer_t* rb_create(NI_S32 s32Len);
ring_buffer_t* rb_create_ex(NI_VOID* pBuff,NI_S32 s32Len);
NI_S32 rb_destory(ring_buffer_t* p);
NI_S32 rb_write(ring_buffer_t* p,NI_VOID* pData,NI_S32 s32Len);
NI_VOID* rb_get_buff(ring_buffer_t* p,NI_S32 s32Len);
NI_S32 rb_ret_buff(ring_buffer_t* p);
NI_S32 rb_release(ring_buffer_t* p);
NI_VOID* rb_read(ring_buffer_t* p,NI_S32* ps32Len);
NI_BOOL rb_is_full(ring_buffer_t* p);
NI_BOOL rb_is_empty(ring_buffer_t* p);
NI_S32 rb_reset(ring_buffer_t* p);

NI_S32 rb_get_read_head(ring_buffer_t* p)    ;
NI_S32 rb_get_read_tail(ring_buffer_t* p)    ;
NI_S32 rb_get_write_head(ring_buffer_t* p)   ;
NI_S32 rb_get_write_tail(ring_buffer_t* p)   ;
NI_S32 rb_get_len(ring_buffer_t* p)          ;
NI_S32 rb_get_data_len(ring_buffer_t* p)     ;
NI_S32 rb_get_rev_len(ring_buffer_t* p)      ;
NI_S32 rb_get_blk_len(ring_buffer_t* p)      ;
NI_VOID* rb_get_memory(ring_buffer_t* p)     ;
NI_S32 rb_dump_file(ring_buffer_t* p,FILE* pf);
NI_S32 rb_dump_memory(ring_buffer_t* p,NI_U8* pu8Mem);

#endif


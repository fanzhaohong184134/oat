#ifndef __WS_CLIENT_H__
#define __WS_CLIENT_H__

typedef struct _USER_DATA_S {
	NI_S32 s32UrlLen;
    NI_VOID* pUrlBuf;
	NI_S32 s32DataLen;
    NI_VOID* pDataBuf;
    struct lws_spa *spa;
} USER_DATA_S;

typedef struct _ws_client_t {
    void* pcontext;
    void* ring_buffer;
#if defined(_WIN32) || defined(_WIN64)
    HANDLE pid;
#else
    int pid;
#endif
} ws_client_t;

void* ws_init(void* rb,const char* ip);
int   ws_exit(void* pctx);
int   ws_stop(void* pctx);
int   ws_send(void* pdata,int len);
int   ws_get_data(void* pdata,int len);
ws_client_t* ws_get_context();
void* ws_get_rb();

#endif

#include <stdio.h>
#include <stdlib.h>
#include "ni_type.h"
#include "ni_debug.h"
#include "libwebsockets.h"
#include "ring_buffer.h"
#include "ws_client.h"

int g_is_on = NI_TRUE;

int main(int argc, const char *argv[])
{
    void* ctx = NULL;
    //ws_get_context();

    FILE* pf = NULL;
    int len;
    int ret;
    unsigned char* buff = NULL;
    char ip[] = "192.168.0.35";
    len = 10*1024*1024;
    /*初始化循环buff*/
    ring_buffer_t* rb = rb_create(len);
    /*初始化码流获取客户端*/
    ctx = ws_init(rb,ip);
    /*创建码流文件*/
    pf = fopen("out.h264","wb");
    if(NULL == pf){
        return 0;
    }
    /*分配缓存*/
    len = 3840*2160/2;
    buff = (NI_U8*)malloc(len);
    if(NULL == buff){
        fclose(pf);
        return 0;
    }

    while(NI_TRUE == g_is_on){
        /*获取循环buffer中的数据*/
        ret = ws_get_data(buff,len);
        /*写入内存*/
        if(ret > 0){
            fwrite(buff+4,ret-4,1,pf);
        }
        else{
            Sleep(1);
        }
    }
    /*停止码流获取*/
    ws_stop(ctx);
    /*释放资源*/
    free(buff);
    fclose(pf);

    return 0;
}

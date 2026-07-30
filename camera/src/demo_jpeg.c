#include <stdio.h>
#include <stdlib.h>
#include "ni_type.h"
#include "ni_debug.h"
#include "sv_api.h"

#define BUFF_LEN 8192

int main(int argc, const char *argv[])
{
    int ret;
    void* ctx = NULL;
    //ws_get_context();

    FILE* pf = NULL;
    int len;
    unsigned char* buff = NULL;
    char ip[] = "192.168.0.38";
    /*设置相机ip*/
    sv_set_server(ip);
    /*创建JPEG文件*/
    pf = fopen("out.jpg","wb");
    if(NULL == pf){
        return 0;
    }
    /*分配缓存*/
    len = 3840*2160/2; /*buffer 为图像尺寸的一半*/
    buff = (NI_U8*)malloc(len);
    if(NULL == buff){
        fclose(pf);
        return 0;
    }

    /*触发相机，获取最新图像*/
    sv_trigger(1);

    /*获取循环buffer中的数据*/
    ret = sv_get_jpeg(buff,len);
    /*写入内存*/
    if(ret > 0){
        fwrite(buff,ret,1,pf);
    }

    /*释放资源*/
    free(buff);
    fclose(pf);

    return 0;
}


#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "ni_type.h"
#include "ni_debug.h"
#include "libwebsockets.h"
#include "ring_buffer.h"
#include "ws_client.h"

#ifdef _WIN32
#include <windows.h>
#else
#include <unistd.h>
#define Sleep(ms) usleep((ms) * 1000)
#endif

int g_is_on = NI_TRUE;

int main(int argc, char *argv[])
{
    void* ctx = NULL;
    FILE* pf = NULL;
    int len;
    int ret;
    unsigned char* buff = NULL;
    char ip[64] = "192.168.0.38";
    int i;
    int total_bytes = 0;

    for (i = 1; i < argc; i++)
    {
        if (strcmp(argv[i], "-i") == 0 && i + 1 < argc)
        {
            strncpy(ip, argv[++i], sizeof(ip) - 1);
            ip[sizeof(ip) - 1] = '\0';
        }
    }

    printf("STATUS Connecting to camera %s ...\n", ip);
    fflush(stdout);

    len = 10 * 1024 * 1024;
    ring_buffer_t* rb = rb_create(len);
    if (NULL == rb)
    {
        printf("STATUS ERROR: rb_create failed\n");
        fflush(stdout);
        return 1;
    }

    ctx = ws_init(rb, ip);
    if (NULL == ctx)
    {
        printf("STATUS ERROR: ws_init failed\n");
        fflush(stdout);
        rb_destory(rb);
        return 1;
    }

    pf = fopen("out.h264", "wb");
    if (NULL == pf)
    {
        printf("STATUS ERROR: fopen out.h264 failed\n");
        fflush(stdout);
        ws_stop(ctx);
        rb_destory(rb);
        return 1;
    }

    len = 3840 * 2160 / 2;
    buff = (unsigned char*)malloc(len);
    if (NULL == buff)
    {
        printf("STATUS ERROR: malloc buffer failed\n");
        fflush(stdout);
        fclose(pf);
        ws_stop(ctx);
        rb_destory(rb);
        return 1;
    }

    printf("STATUS H264 stream started\n");
    fflush(stdout);

    while (NI_TRUE == g_is_on)
    {
        ret = ws_get_data(buff, len);
        if (ret > 4)
        {
            fwrite(buff + 4, ret - 4, 1, pf);
            fflush(pf);
            total_bytes += (ret - 4);

            if (total_bytes % (1024 * 1024) < (ret - 4))
            {
                printf("STATUS Streamed %d bytes\n", total_bytes);
                fflush(stdout);
            }
        }
        else
        {
            Sleep(1);
        }
    }

    ws_stop(ctx);
    free(buff);
    fclose(pf);
    rb_destory(rb);

    printf("STATUS H264 stream stopped\n");
    fflush(stdout);
    return 0;
}

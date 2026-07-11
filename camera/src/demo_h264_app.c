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
#include <fcntl.h>
#include <io.h>
#else
#include <unistd.h>
#define Sleep(ms) usleep((ms) * 1000)
#endif

int g_is_on = NI_TRUE;

int main(int argc, char *argv[])
{
    void* ctx = NULL;
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

#ifdef _WIN32
    _setmode(_fileno(stdout), _O_BINARY);
#endif

    fprintf(stderr, "STATUS Connecting to camera %s ...\n", ip);
    fflush(stderr);

    len = 10 * 1024 * 1024;
    ring_buffer_t* rb = rb_create(len);
    if (NULL == rb)
    {
        fprintf(stderr, "STATUS ERROR: rb_create failed\n");
        fflush(stderr);
        return 1;
    }

    ctx = ws_init(rb, ip);
    if (NULL == ctx)
    {
        fprintf(stderr, "STATUS ERROR: ws_init failed\n");
        fflush(stderr);
        rb_destory(rb);
        return 1;
    }

    len = 3840 * 2160 / 2;
    buff = (unsigned char*)malloc(len);
    if (NULL == buff)
    {
        fprintf(stderr, "STATUS ERROR: malloc buffer failed\n");
        fflush(stderr);
        ws_stop(ctx);
        rb_destory(rb);
        return 1;
    }

    fprintf(stderr, "STATUS H264 stream started\n");
    fflush(stderr);

    while (NI_TRUE == g_is_on)
    {
        ret = ws_get_data(buff, len);
        if (ret > 4)
        {
            fwrite(buff + 4, 1, ret - 4, stdout);
            fflush(stdout);
            total_bytes += (ret - 4);

            if (total_bytes % (1024 * 1024) < (ret - 4))
            {
                fprintf(stderr, "STATUS Streamed %d bytes\n", total_bytes);
                fflush(stderr);
            }
        }
        else
        {
            Sleep(1);
        }
    }

    ws_stop(ctx);
    free(buff);
    rb_destory(rb);

    fprintf(stderr, "STATUS H264 stream stopped\n");
    fflush(stderr);
    return 0;
}

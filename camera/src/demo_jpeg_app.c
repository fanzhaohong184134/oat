#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <windows.h>
#include "ni_type.h"
#include "ni_debug.h"
#include "sv_api.h"

#define BUFF_LEN (3840*2160/2)
#define DEFAULT_INTERVAL 5
#define DEFAULT_PREFIX    "photo"
#define DEFAULT_IP        "192.168.0.38"

static void print_usage(const char *prog)
{
    printf("Usage: %s [-i ip_addr] [-d time_int] [-f jpeg_name] [-n number]\n", prog);
    printf("  -i ip_addr     : camera IP address (default: %s)\n", DEFAULT_IP);
    printf("  -d time_int    : capture interval in seconds (default: %d)\n", DEFAULT_INTERVAL);
    printf("  -f jpeg_name   : output filename prefix (default: %s)\n", DEFAULT_PREFIX);
    printf("  -n number      : number of captures (default: unlimited)\n");
}

int main(int argc, const char *argv[])
{
    int ret;
    int i;
    FILE *pf = NULL;
    int len;
    unsigned char *buff = NULL;
    char ip[64] = {0};
    char prefix[128] = {0};
    int interval = DEFAULT_INTERVAL;
    int number = -1; /* -1 means unlimited */
    char filename[256];
    time_t ts;
    int count = 0;
    NI_BOOL ip_set = NI_FALSE;

    /* 确保stdout在被管道重定向时也能及时刷新（默认全缓冲会导致C#收不到输出） */
    setvbuf(stdout, NULL, _IONBF, 0);

    /* parse command line arguments */
    for (i = 1; i < argc; i++) {
        if (strcmp(argv[i], "-i") == 0 && (i + 1) < argc) {
            strncpy(ip, argv[++i], sizeof(ip) - 1);
            ip_set = NI_TRUE;
        }
        else if (strcmp(argv[i], "-d") == 0 && (i + 1) < argc) {
            interval = atoi(argv[++i]);
            if (interval <= 0) {
                interval = DEFAULT_INTERVAL;
            }
        }
        else if (strcmp(argv[i], "-f") == 0 && (i + 1) < argc) {
            strncpy(prefix, argv[++i], sizeof(prefix) - 1);
        }
        else if (strcmp(argv[i], "-n") == 0 && (i + 1) < argc) {
            number = atoi(argv[++i]);
            if (number <= 0) {
                number = -1;
            }
        }
        else {
            print_usage(argv[0]);
            return 0;
        }
    }

    if (!ip_set) {
        strncpy(ip, DEFAULT_IP, sizeof(ip) - 1);
    }

    if (prefix[0] == '\0') {
        strncpy(prefix, DEFAULT_PREFIX, sizeof(prefix) - 1);
    }

    printf("Camera IP  : %s\n", ip);
    printf("Interval   : %d s\n", interval);
    printf("Prefix     : %s\n", prefix);
    if (number > 0) {
        printf("Count      : %d\n", number);
    } else {
        printf("Count      : unlimited\n");
    }

    /* set camera ip */
    sv_set_server(ip);

    /* allocate buffer */
    len = BUFF_LEN;
    buff = (NI_U8 *)malloc(len);
    if (NULL == buff) {
        printf("Failed to allocate buffer\n");
        return 0;
    }

    /* capture loop */
    while (1) {
        /* check capture count limit */
        if (number > 0 && count >= number) {
            break;
        }

        /* build filename: prefix_timestamp.jpeg */
        ts = time(NULL);
        sprintf(filename, "%s_%lld.jpeg", prefix, (long long)ts);

        /* create jpeg file */
        pf = fopen(filename, "wb");
        if (NULL == pf) {
            printf("Failed to create file: %s\n", filename);
            break;
        }

        /* trigger camera to get latest image */
        sv_trigger(1);

        /* get jpeg data from ring buffer */
        ret = sv_get_jpeg(buff, len);

        /* write to file */
        if (ret > 0) {
            fwrite(buff, ret, 1, pf);
            count++;
            printf("[%d] Saved: %s (%d bytes)\n", count, filename, ret);
        } else {
            printf("Failed to get jpeg data, ret=%d\n", ret);
        }

        fclose(pf);
        pf = NULL;

        /* wait for the next capture */
        Sleep(interval * 1000);
    }

    /* release resources */
    free(buff);

    printf("Capture finished, total %d images.\n", count);

    return 0;
}
/*****************************************************************************
 * demo_h264_app.c - 实时视频流预览程序
 * 
 * 功能：通过相机SDK高频获取JPEG图像，用于实时预览
 * 输出：FRAME <filename> - 每帧图像路径
 *       STATUS <message> - 状态信息
 *
 * 用法：demo_h264_app.exe -i <ip> -o <output_dir> [-q <quality>] [-f <fps>]
 ****************************************************************************/

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#ifdef _WIN32
#include <windows.h>
#include <direct.h>
#define mkdir(d) _mkdir(d)
#define sleep_ms(ms) Sleep(ms)
#else
#include <unistd.h>
#include <sys/stat.h>
#define mkdir(d) mkdir(d, 0755)
#define sleep_ms(ms) usleep((ms)*1000)
#endif

#include "ni_type.h"
#include "ni_debug.h"
#include "sv_api.h"

/* 全局控制标志 */
static volatile int g_is_running = 1;

#ifdef _WIN32
/* Windows控制台关闭处理 */
static BOOL WINAPI console_handler(DWORD signal)
{
    if (signal == CTRL_C_EVENT || signal == CTRL_BREAK_EVENT) {
        g_is_running = 0;
        return TRUE;
    }
    return FALSE;
}
#endif

int main(int argc, char* argv[])
{
    char ip[64] = "192.168.0.38";
    char output_dir[256] = "preview_frames";
    int quality = 80;           /* JPEG质量 */
    int target_fps = 15;        /* 目标帧率 */
    int frame_interval_ms;      /* 帧间隔毫秒 */
    int i;
    
    /* 解析命令行参数 */
    for (i = 1; i < argc; i++) {
        if (strcmp(argv[i], "-i") == 0 && i + 1 < argc) {
            strncpy(ip, argv[++i], sizeof(ip) - 1);
            ip[sizeof(ip) - 1] = '\0';
        }
        else if (strcmp(argv[i], "-o") == 0 && i + 1 < argc) {
            strncpy(output_dir, argv[++i], sizeof(output_dir) - 1);
            output_dir[sizeof(output_dir) - 1] = '\0';
        }
        else if (strcmp(argv[i], "-q") == 0 && i + 1 < argc) {
            quality = atoi(argv[++i]);
            if (quality < 1) quality = 1;
            if (quality > 100) quality = 100;
        }
        else if (strcmp(argv[i], "-f") == 0 && i + 1 < argc) {
            target_fps = atoi(argv[++i]);
            if (target_fps < 1) target_fps = 1;
            if (target_fps > 30) target_fps = 30;
        }
    }
    
    /* 计算帧间隔 */
    frame_interval_ms = 1000 / target_fps;
    
#ifdef _WIN32
    /* 设置控制台关闭处理 */
    SetConsoleCtrlHandler(console_handler, TRUE);
#endif
    
    /* 创建输出目录 */
    mkdir(output_dir);
    
    /* 设置相机服务器IP */
    sv_set_server(ip);
    
    printf("STATUS Connecting to camera %s ...\n", ip);
    fflush(stdout);
    
    /* 分配JPEG缓冲区 (4MB应该足够) */
    int jpeg_buf_size = 4 * 1024 * 1024;
    unsigned char* jpeg_buf = (unsigned char*)malloc(jpeg_buf_size);
    if (!jpeg_buf) {
        printf("STATUS ERROR: Failed to allocate JPEG buffer\n");
        fflush(stdout);
        return 1;
    }
    
    /* 主循环：持续获取图像 */
    int frame_count = 0;
    int failed_count = 0;
    time_t last_status_time = time(NULL);
    
    printf("STATUS Preview started, target FPS: %d\n", target_fps);
    fflush(stdout);
    
    while (g_is_running) {
        /* 触发拍照 */
        int ret = sv_trigger(1);
        if (ret < 0) {
            failed_count++;
            if (failed_count > 10) {
                printf("STATUS ERROR: Too many trigger failures\n");
                fflush(stdout);
                break;
            }
            sleep_ms(100);
            continue;
        }
        
        /* 获取JPEG图像 */
        ret = sv_get_jpeg(jpeg_buf, jpeg_buf_size);
        if (ret <= 0) {
            /* 获取失败，可能是相机还在处理 */
            sleep_ms(10);
            continue;
        }
        
        /* 重置失败计数 */
        failed_count = 0;
        
        /* 保存图像文件 */
        frame_count++;
        char filename[512];
        snprintf(filename, sizeof(filename), "%s/preview_%010d.jpg", 
                 output_dir, frame_count);
        
        FILE* fp = fopen(filename, "wb");
        if (fp) {
            fwrite(jpeg_buf, ret, 1, fp);
            fclose(fp);
            
            /* 输出帧信息 */
            printf("FRAME %s %d %d\n", filename, ret, quality);
            fflush(stdout);
        }
        
        /* 定期输出状态（每5秒） */
        time_t now = time(NULL);
        if (now - last_status_time >= 5) {
            printf("STATUS FPS: %.1f, Frames: %d\n", 
                   (double)frame_count / (now - last_status_time + 5), 
                   frame_count);
            fflush(stdout);
            last_status_time = now;
        }
        
        /* 控制帧率 */
        sleep_ms(frame_interval_ms);
    }
    
    /* 清理资源 */
    free(jpeg_buf);
    
    printf("STATUS Preview stopped. Total frames: %d\n", frame_count);
    fflush(stdout);
    
    return 0;
}

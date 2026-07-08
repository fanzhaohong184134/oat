#ifndef __NI_DEBUG_H__
#define __NI_DEBUG_H__

#include <assert.h>

#define ni_level_cri   0
#define ni_level_error 1
#define ni_level_debug 2
#define ni_level_info  3

#define ni_trace(ID,LEVEL,s,...) \
	do{ \
        int _b_show = (LEVEL)<= ni_level_debug;\
        if(_b_show){ \
	    printf("[%10s, %2d] "s,__FUNCTION__,__LINE__,##__VA_ARGS__); \
        }\
	}while(0);

#define NI_ASSERT(c) \
    do { \
        if(!(c)){ \
            printf("[%10s, %2d] ASSERT Failed:%s\n",__FUNCTION__,__LINE__,#c); \
            assert(c); \
        }\
    }while(0);

#endif /*__DEBUG_H__*/


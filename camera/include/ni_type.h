#ifndef __NI_TYPE_H__
#define __NI_TYPE_H__

#ifndef _M_IX86
typedef unsigned long long      NI_U64;
typedef long long               NI_S64;
#else
typedef unsigned __int64        NI_U64;
typedef __int64                 NI_S64;
#endif

#define NI_S32 int
#define NI_U32 unsigned int
#define NI_S16 short
#define NI_U16 unsigned short
#define NI_S8  char
#define NI_CHAR  char
#define NI_U8  unsigned char
#define NI_VOID void
#define NI_BOOL int
#define NI_NULL 0
#define NI_DOUBLE double
#define NI_FLOAT float

#define NI_TRUE 1
#define NI_FALSE 0

#define NI_SUCCESS 0
#define NI_FAILURE (-1)

#endif /*_TYPE_H_*/

#ifndef __NI_COMM_H__
#define __NI_COMM_H__

#define PI 3.14159265358
#define EDGE_ANGLE_PI 1024
#define EDGE_ANGLE_PI2 2048

#ifndef MAX
#define MAX(a,b) ((a) > (b) ? (a) : (b)) 
#endif
#ifndef MIN
#define MIN(a,b) ((a) < (b) ? (a) : (b)) 
#endif
#define MEAM(a,b,c) ((a) > (b) ? MIN(a,c) : MIN(b,c))
#define ALIGN(a,b) (((a) + (b) -1) / (b))*(b)
#ifndef ABS
#define ABS(a) ((a) < 0 ? -(a) : (a))
#endif
#define CLIP3(x,min,max)         ( (x)< (min) ? (min) : ((x)>(max)?(max):(x)) )
#ifndef MAX3
#define MAX3(a,b,c) ((a) > (b) ? ((a) > (c) ? (a):(c)) : ((b) > (c) ? (b):(c))) 
#endif
#ifndef MIN3
#define MIN3(a,b,c) ((a) < (b) ? ((a) < (c) ? (a):(c)) : ((b) < (c) ? (b):(c))) 
#endif

#define BIG_ENDIAN16(x) ((((x)&0xFF00)>>8) |(((x)&0x00FF)<<8))
#define BIG_ENDIAN32(x) ((((x)&0xFF000000)>>24) |(((x)&0x00FF0000)>>8) |(((x)&0xFF00)<<8) |(((x)&0x00FF)<<24))

#endif /*__NI_COMM_H__*/

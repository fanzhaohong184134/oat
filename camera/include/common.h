#ifndef __COMMON_H__
#define __COMMON_H__

#define BIG_ENDIAN16(x) ((((x)&0xFF00)>>8) |(((x)&0x00FF)<<8))
#define BIG_ENDIAN32(x) ((((x)&0xFF000000)>>24) |(((x)&0x00FF0000)>>8) |(((x)&0xFF00)<<8) |(((x)&0x00FF)<<24))

#endif /*__COMMON_H__*/


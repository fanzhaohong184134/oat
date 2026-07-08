#ifndef __SV_API_H__
#define __SV_API_H__

NI_S32 sv_set_server    (NI_CHAR* ip);
NI_S32 sv_trigger       (NI_S32 val);
NI_S32 sv_set_trigger   (NI_S32 val);
NI_S32 sv_set_light     (NI_S32 val);
NI_S32 sv_set_contrast  (NI_S32 val);
NI_S32 sv_set_saturation(NI_S32 val);
NI_S32 sv_set_again     (NI_S32 val);
NI_S32 sv_set_dgain     (NI_S32 val);
NI_S32 sv_set_exp_time  (NI_S32 val);
NI_S32 sv_set_max_exp   (NI_S32 val);
NI_S32 sv_set_bit_rate  (NI_S32 val);
NI_S32 sv_set_frame_rate(NI_S32 val);
NI_S32 sv_set_gop       (NI_S32 val);
NI_S32 sv_set_width     (NI_S32 val);
NI_S32 sv_set_height    (NI_S32 val);

NI_S32 sv_get_jpeg(NI_VOID* buf,NI_S32 len);
NI_S32 sv_get_yuv(NI_VOID* buf,NI_S32 len);

#endif /* __SV_API_H__ */
    

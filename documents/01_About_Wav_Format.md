# WAV フォーマットについて

## 簡単に説明すると

WAV は Microsoft によって定められたファイルフォーマットで、音声データは RIFF 形式で格納される。RIFF(Resource Interchange File Format) 形式とは、画像や音声などのデータを一つのファイルに格納するための共通フォーマットである。

## RIFF 形式について

1991 年に Microsoft と IBM が定めたフォーマットで、タグ付きのデータを格納するための汎用メタファイル形式である。RIFF ファイルは「チャンク」と呼ばれるものの集まりであり、すべてのチャンクは次のような形式である。

| フィールド       |  バイト数   | 内容                                                                         |
| :--------------- | :---------: | :--------------------------------------------------------------------------- |
| 識別子           |   4 Bytes   | ASCII で `RIFF`、`fmt`、`data` など                                          |
| チャンクの長さ   |   4 Bytes   | 次の項目以降のチャンクの長さを表す。符号なしのリトルエンディアン 32 bit 整数 |
| 可変長フィールド |   n Bytes   | チャンクデータ本体。長さは前のフィールドによって示される                     |
| パディング       | 0 or 1 Byte | チャンクの長さが奇数バイトの場合は 1 バイト追加される。                      |

チャンク識別子が `RIFF` もしくは `LIST` の場合は、チャンク内にサブチャンクを含むことができる。

| 項目           | バイト長 | 説明                |
| :------------- | :------: | :------------------ |
| フォームタイプ | 4 Bytes  | `WAVE`、`AVI ` など |

## WAV のデータ

### RIFF チャンク(RIFF chunk)

ファイルの先頭は ASCII の `RIFF` で始まる。16 進数では `52 49 46 46` で、これを読み取ってファイルが RIFF であると判断する。

RIFF チャンクのフォーマットは次のとおりである。

| フィールド     | バイト数 | 内容                                            |
| :------------- | :------: | :---------------------------------------------- |
| 識別子         | 4 Bytes  | `RIFF`                                          |
| チャンクの長さ | 4 Bytes  | `WAVE` の長さ(つまり 4) + WAVE のチャンクの長さ |
| 識別子         | 4 Bytes  | `WAVE`                                          |
| チャンクの長さ | n Bytes  | 書式情報とデータ                                |

RIFF チャンクの後には JUNK チャンク もしくは fmt チャンク が続く。

### JUNK チャンク(JUNK chunk)

識別子は `JUNK` で、音声には関係ないデータを格納する。具体的なデータに何を格納しているか、用途は何なのかについては不明。

| フィールド     | バイト数 | 内容                               |
| :------------- | :------: | :--------------------------------- |
| 識別子         | 4 Bytes  | `JUNK`                             |
| チャンクの長さ | 4 Bytes  | 次のバイトから始まるチャンクの長さ |
| データ         | n Bytes  | すべて `0x00`                      |

### fmt チャンク(fmt chunk)

| フィールド                 | バイト数 | 内容                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| :------------------------- | :------: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 識別子                     | 4 Bytes  | `fmt `                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| チャンクの長さ             | 4 Bytes  | 16 進数で `10 00 00 00(DEC 16)`、`HEX 12 00 00 00(DEC 18)` もしくは `28 00 00 00(DEC 40)`                                                                                                                                                                                                                                                                                                                                                     |
| タグ                       | 2 Bytes  | フォーマットを表す列挙値。以降のフォーマットタグを参照のこと                                                                                                                                                                                                                                                                                                                                                                                  |
| チャンネル数               | 2 Bytes  | 音声で使用しているチャンネル数                                                                                                                                                                                                                                                                                                                                                                                                                |
| 1 秒あたりのサンプル数     | 4 Bytes  | サンプル/秒で表すサンプルレート。数値については、以降のサンプルレートを参照のこと                                                                                                                                                                                                                                                                                                                                                             |
| 1 秒あたりの平均バイト数   | 4 Bytes  | 平均データ転送レート(バイト/秒)。PCM フォーマットの場合は、"サンプルレート ＝ 平均データ転送レート ＝ ブロックアラインメント" でなければならない。                                                                                                                                                                                                                                                                                            |
| ブロックアラインメント     | 2 Bytes  | ブロックアラインメント(単位はバイト)。タグで指定されたフォーマットのデータ採用構成単位。PCM もしくは拡張(`WAVE_FORMAT_EXTENSIBLE`)の場合は、 **_"ブロックアラインメント = (チャンネル数 × 1 秒あたりのサンプル数) ÷ 8"_** が成り立たなければならない。 ソフトウェアは一度に複数のブロックアライメントで指定されたバイトのデータを扱う必要がある。デバイスに対するデータの書き込みと読み取りは、常にブロックの先頭から開始しなければならない。 |
| 1 サンプルあたりのビット数 | 2 Bytes  | タグが PCM の場合は、この値は `8` あるいは `10(DEC 16)` の必要がある。                                                                                                                                                                                                                                                                                                                                                                        |
| 拡張データのサイズ(バイト) | 2 Bytes  | `0` もしくは `16(DEC 22)`                                                                                                                                                                                                                                                                                                                                                                                                                     |
| 1 サンプルあたりのビット数 | 2 Bytes  | サンプルの解像度。通常は 1 サンプルあたりのビット数に等しい。                                                                                                                                                                                                                                                                                                                                                                                 |
| チャンネルマスク           | 4 Bytes  | スピーカー位置へのストリーム内のチャンネル割当を指定するビットマスク。                                                                                                                                                                                                                                                                                                                                                                        |
| サブフォーマット           | 16 Bytes | GUID。データのサブフォーマット。                                                                                                                                                                                                                                                                                                                                                                                                              |

#### フォーマットタグ

以下の 16 進数の数値が、データフォーマットを表す。

```yaml
0x0000: unknown
0x0001: pcm
0x0002: adpcm
0x0003: ieee_float
0x0004: vselp
0x0005: ibm_cvsd
0x0006: alaw
0x0007: mulaw
0x0008: dts
0x0009: drm
0x000a: wmavoice9
0x000b: wmavoice10
0x0010: oki_adpcm
0x0011: dvi_adpcm
0x0012: mediaspace_adpcm
0x0013: sierra_adpcm
0x0014: g723_adpcm
0x0015: digistd
0x0016: digifix
0x0017: dialogic_oki_adpcm
0x0018: mediavision_adpcm
0x0019: cu_codec
0x001a: hp_dyn_voice
0x0020: yamaha_adpcm
0x0021: sonarc
0x0022: dspgroup_truespeech
0x0023: echosc1
0x0024: audiofile_af36
0x0025: aptx
0x0026: audiofile_af10
0x0027: prosody_1612
0x0028: lrc
0x0030: dolby_ac2
0x0031: gsm610
0x0032: msnaudio
0x0033: antex_adpcme
0x0034: control_res_vqlpc
0x0035: digireal
0x0036: digiadpcm
0x0037: control_res_cr10
0x0038: nms_vbxadpcm
0x0039: cs_imaadpcm
0x003a: echosc3
0x003b: rockwell_adpcm
0x003c: rockwell_digitalk
0x003d: xebec
0x0040: g721_adpcm
0x0041: g728_celp
0x0042: msg723
0x0043: intel_g723_1
0x0044: intel_g729
0x0045: sharp_g726
0x0050: mpeg
0x0052: rt24
0x0053: pac
0x0055: mpeglayer3
0x0059: lucent_g723
0x0060: cirrus
0x0061: espcm
0x0062: voxware
0x0063: canopus_atrac
0x0064: g726_adpcm
0x0065: g722_adpcm
0x0066: dsat
0x0067: dsat_display
0x0069: voxware_byte_aligned
0x0070: voxware_ac8
0x0071: voxware_ac10
0x0072: voxware_ac16
0x0073: voxware_ac20
0x0074: voxware_rt24
0x0075: voxware_rt29
0x0076: voxware_rt29hw
0x0077: voxware_vr12
0x0078: voxware_vr18
0x0079: voxware_tq40
0x007a: voxware_sc3
0x007b: voxware_sc3_1
0x0080: softsound
0x0081: voxware_tq60
0x0082: msrt24
0x0083: g729a
0x0084: mvi_mvi2
0x0085: df_g726
0x0086: df_gsm610
0x0088: isiaudio
0x0089: onlive
0x008a: multitude_ft_sx20
0x008b: infocom_its_g721_adpcm
0x008c: convedia_g729
0x008d: congruency
0x0091: sbc24
0x0092: dolby_ac3_spdif
0x0093: mediasonic_g723
0x0094: prosody_8kbps
0x0097: zyxel_adpcm
0x0098: philips_lpcbb
0x0099: packed
0x00a0: malden_phonytalk
0x00a1: racal_recorder_gsm
0x00a2: racal_recorder_g720_a
0x00a3: racal_recorder_g723_1
0x00a4: racal_recorder_tetra_acelp
0x00b0: nec_aac
0x00ff: raw_aac1
0x0100: rhetorex_adpcm
0x0101: irat
0x0111: vivo_g723
0x0112: vivo_siren
0x0120: philips_celp
0x0121: philips_grundig
0x0123: digital_g723
0x0125: sanyo_ld_adpcm
0x0130: siprolab_aceplnet
0x0131: siprolab_acelp4800
0x0132: siprolab_acelp8v3
0x0133: siprolab_g729
0x0134: siprolab_g729a
0x0135: siprolab_kelvin
0x0136: voiceage_amr
0x0140: g726adpcm
0x0141: dictaphone_celp68
0x0142: dictaphone_celp54
0x0150: qualcomm_purevoice
0x0151: qualcomm_halfrate
0x0155: tubgsm
0x0160: msaudio1
0x0161: wmaudio2
0x0162: wmaudio3
0x0163: wmaudio_lossless
0x0164: wmaspdif
0x0170: unisys_nap_adpcm
0x0171: unisys_nap_ulaw
0x0172: unisys_nap_alaw
0x0173: unisys_nap_16k
0x0174: sycom_acm_syc008
0x0175: sycom_acm_syc701_g726l
0x0176: sycom_acm_syc701_celp54
0x0177: sycom_acm_syc701_celp68
0x0178: knowledge_adventure_adpcm
0x0180: fraunhofer_iis_mpeg2_aac
0x0190: dts_ds
0x0200: creative_adpcm
0x0202: creative_fastspeech8
0x0203: creative_fastspeech10
0x0210: uher_adpcm
0x0215: ulead_dv_audio
0x0216: ulead_dv_audio_1
0x0220: quarterdeck
0x0230: ilink_vc
0x0240: raw_sport
0x0241: esst_ac3
0x0249: generic_passthru
0x0250: ipi_hsx
0x0251: ipi_rpelp
0x0260: cs2
0x0270: sony_scx
0x0271: sony_scy
0x0272: sony_atrac3
0x0273: sony_spc
0x0280: telum_audio
0x0281: telum_ia_audio
0x0285: norcom_voice_systems_adpcm
0x0300: fm_towns_snd
0x0350: micronas
0x0351: micronas_celp833
0x0400: btv_digital
0x0401: intel_music_coder
0x0402: indeo_audio
0x0450: qdesign_music
0x0500: on2_vp7_audio
0x0501: on2_vp6_audio
0x0680: vme_vmpcm
0x0681: tpc
0x08ae: lightwave_lossless
0x1000: oligsm
0x1001: oliadpcm
0x1002: olicelp
0x1003: olisbc
0x1004: oliopr
0x1100: lh_codec
0x1101: lh_codec_celp
0x1102: lh_codec_sbc8
0x1103: lh_codec_sbc12
0x1104: lh_codec_sbc16
0x1400: norris
0x1401: isiaudio_2
0x1500: soundspace_musicompress
0x1600: mpeg_adts_aac
0x1601: mpeg_raw_aac
0x1602: mpeg_loas
0x1608: nokia_mpeg_adts_aac
0x1609: nokia_mpeg_raw_aac
0x160a: vodafone_mpeg_adts_aac
0x160b: vodafone_mpeg_raw_aac
0x1610: mpeg_heaac
0x181c: voxware_rt24_speech
0x1971: sonicfoundry_lossless
0x1979: innings_telecom_adpcm
0x1c07: lucent_sx8300p
0x1c0c: lucent_sx5363s
0x1f03: cuseeme
0x1fc4: ntcsoft_alf2cm_acm
0x2000: dvm
0x2001: dts2
0x3313: makeavis
0x4143: divio_mpeg4_aac
0x4201: nokia_adaptive_multirate
0x4243: divio_g726
0x434c: lead_speech
0x564c: lead_vorbis
0x5756: wavpack_audio
0x674f: ogg_vorbis_mode_1
0x6750: ogg_vorbis_mode_2
0x6751: ogg_vorbis_mode_3
0x676f: ogg_vorbis_mode_1_plus
0x6770: ogg_vorbis_mode_2_plus
0x6771: ogg_vorbis_mode_3_plus
0x7000: threecom_nbx
0x706d: faad_aac
0x7361: amr_nb
0x7362: amr_wb
0x7363: amr_wp
0x7a21: gsm_amr_cbr
0x7a22: gsm_amr_vbr_sid
0xa100: comverse_infosys_g723_1
0xa101: comverse_infosys_avqsbc
0xa102: comverse_infosys_sbc
0xa103: symbol_g729_a
0xa104: voiceage_amr_wb
0xa105: ingenient_g726
0xa106: mpeg4_aac
0xa107: encore_g726
0xa108: zoll_asao
0xa109: speex_voice
0xa10a: vianix_masc
0xa10b: wm9_spectrum_analyzer
0xa10c: wmf_spectrum_anayzer
0xa10d: gsm_610
0xa10e: gsm_620
0xa10f: gsm_660
0xa110: gsm_690
0xa111: gsm_adaptive_multirate_wb
0xa112: polycom_g722
0xa113: polycom_g728
0xa114: polycom_g729_a
0xa115: polycom_siren
0xa116: global_ip_ilbc
0xa117: radiotime_time_shift_radio
0xa118: nice_aca
0xa119: nice_adpcm
0xa11a: vocord_g721
0xa11b: vocord_g726
0xa11c: vocord_g722_1
0xa11d: vocord_g728
0xa11e: vocord_g729
0xa11f: vocord_g729_a
0xa120: vocord_g723_1
0xa121: vocord_lbc
0xa122: nice_g728
0xa123: frace_telecom_g729
0xa124: codian
0xf1ac: flac
0xfffe: extensible
0xffff: development
```

#### サンプルレート

|      値       | サンプルレート |
| :-----------: | -------------: |
| `0x0000 2b11` |          11025 |
| `0x0000 5622` |          22050 |
| `0x0000 7d00` |          32000 |
| `0x0000 ac44` |          44100 |
| `0x0000 bb80` |          48000 |

### fact チャンク(fact chunk)

fact チャンクは圧縮形式の非 PCM 形式であることを示す。

| フィールド | バイト数 | 内容 |
| :--------- | :------: | :--------- |
| 識別子    | 4 Bytes | `fact` |
| チャンクの長さ | 4 Bytes | 最小で `4` |
| サンプルの長さ | 4 Bytes | チャンネルごとのサンプル数 |

### data チャンク(data chunk)

data チャンクには実際のサンプルデータが格納されている。

| フィールド | バイト数 | 内容 |
| :-------- | :----: | :------ |
| 識別子   | 4 Bytes | `data` |
| チャンクの長さ | 4 Bytes | サンプルデータの長さ |
| サンプルデータ | n Bytes | 実際のデータ(データサイズはサンプルあたりのビット長に依る) |
| パディング | 0 もしくは 1 Bytes | チャンクの長さが奇数の場合はパディングが 1 バイト入る |

### INFO チャンク(INFO chunk)

アーティストや著作権などのメタ情報を示す。フォーマットについての個々の取り扱いについては、ここでは省略する。
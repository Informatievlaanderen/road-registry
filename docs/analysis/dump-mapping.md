# Introduction

Below you'll find a mapping and description  of the various dBase III and shape files.

## Tables

### AttRijstroken

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| RS_OIDN | rijstrokenID | - |
| WS_OIDN | wegsegmentID | - |
| WS_GIDN | wegsegmentGeometrieVersieID | - |
| AANTAL | aantal | - |
| RICHTING | richtingCode | - |
| LBLRICHT | richtingLabel | - |
| VANPOS | vanPositie | - |
| TOTPOS | totPositie | - |
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| RS_OIDN | Number | 15 | 0 |
| WS_OIDN | Number | 15 | 0 |
| WS_GIDN | Character | 18 | 0 |
| AANTAL | Number | 2 | 0 |
| RICHTING | Number | 2 | 0 |
| LBLRICHT | Character | 64 | 0 |
| VANPOS | Number | 9 | 3 |
| TOTPOS | Number | 9 | 3 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

### AttWegbreedte

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| WB_OIDN | wegbreedteID | - |
| WS_OIDN | wegsegmentID | - |
| WS_GIDN | wegsegmentGeometrieVersieID | - |
| BREEDTE | breedte | - |
| VANPOS | vanPositie | - |
| TOTPOS | totPositie | - |
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| WB_OIDN | Number | 15 | 0 |
| WS_OIDN | Number | 15 | 0 |
| WS_GIDN | Character | 18 | 0 |
| BREEDTE | Number | 2 | 0 |
| VANPOS | Number | 9 | 3 |
| TOTPOS | Number | 9 | 3 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

### AttWegverharding

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| WV_OIDN | wegverhardingID | - |
| WS_OIDN | wegsegmentID | - |
| WS_GIDN | wegsegmentGeometrieVersieID | - |
| TYPE | typeCode | - |
| LBLTYPE | typeLabel | - |
| VANPOS | vanPositie | - |
| TOTPOS | totPositie | - |
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| WV_OIDN | Number | 15 | 0 |
| WS_OIDN | Number | 15 | 0 |
| WS_GIDN | Character | 18 | 0 |
| TYPE | Number | 2 | 0 |
| LBLTYPE | Character | 64 | 0 |
| VANPOS | Number | 9 | 3 |
| TOTPOS | Number | 9 | 3 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

### AttEuropweg

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| EU_OIDN | EuropeseWegID | - |
| WS_OIDN | wegsegmentID | - |
| EUNUMMER | euWegnummer | - |            
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| EU_OIDN | Number | 15 | 0 |
| WS_OIDN | Number | 15 | 0 |
| EUNUMMER | Character | 4 | 0 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

### AttNationweg

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| NW_OIDN | nationaleWegID | |
| WS_OIDN | wegsegmentID |
| IDENT2 | ident2 |            
| BEGINTIJD | begintijd |
| BEGINORG | beginorganisatieCode |
| LBLBGNORG | beginorganisatieLabel |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| NW_OIDN | Number | 15 | 0 |
| WS_OIDN | Number | 15 | 0 |
| IDENT2 | Character | 8 | 0 |         
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

### AttGenumweg

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| GW_OIDN | genummerdeWegID | - |
| WS_OIDN | wegsegmentID | - |
| IDENT8 | ident8 | - |
| RICHTING | richtingCode | - |
| LBLRICHT | richtingLabel | - |
| VOLGNUMMER | volgnummer | - |            
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| GW_OIDN | Number | 15 | 0 |
| WS_OIDN | Number | 15 | 0 |
| IDENT8 | Character | 8 | 0 |
| RICHTING | Number | 2 | 0 |
| LBLRICHT | Character | 64 | 0 |
| VOLGNUMMER | Number | 5 | 0 |            
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

### RltOgkruising

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| OK_OIDN | ongelijkgrondseKruisingID | - |
| TYPE | typeCode | - |
| LBLTYPE | typeLabel | - |
| BO_WS_OIDN | bovenWegsegmentID | - |
| ON_WS_OIDN | onderWegsegmentID | - |
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| OK_OIDN | Number | 15 | 0 |
| TYPE | Number | 2 | 0 |
| LBLTYPE | Character | 64 | 0 |
| BO_WS_OIDN | Number | 15 | 0 |
| ON_WS_OIDN | Number | 15 | 0 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

### RefpuntLktType

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| TYPE | code | - |
| LBLTYPE | label | - |
| DEFTYPE | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| TYPE | Number | 2 | 0 |
| LBLTYPE | Character | 64 | 0 |
| DEFTYPE | Character | 255 | 0 |

### LstOrg

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| ORG | code | - |
| LBLORG | label | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| ORG | Character | 18 | 0 |
| LBLORG | Character | 64 | 0 |

### WegsegmentLktWegcat

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| WEGCAT | code | - |
| LBLWEGCAT | label | - |
| DEFWEGCAT | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| WEGCAT | Character | 5 | 0 |
| LBLWEGCAT | Character | 64 | 0 |
| DEFWEGCAT | Character | 255 | 0 |

### GenumwegLktRichting

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| RICHTING | code | - |
| LBLRICHT | label | - |
| DEFRICHT | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| RICHTING | Number | 2 | 0 |
| LBLRICHT | Character | 64 | 0 |
| DEFRICHT | Character | 255 | 0 |

### WegverhardLktType

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| TYPE | code | - |
| LBLTYPE | label | - |
| DEFTYPE | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| TYPE | Number | 2 | 0 |
| LBLTYPE | Character | 64 | 0 |
| DEFTYPE | Character | 255 | 0 |

### WegknoopLktType

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| TYPE | code | - |
| LBLTYPE | label | - |
| DEFTYPE | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| TYPE | Number | 2 | 0 |
| LBLTYPE | Character | 64 | 0 |
| DEFTYPE | Character | 255 | 0 |

### WegsegmentLktMorf

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| MORF | code | - |
| LBLMORF | label | - |
| DEFMORF | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| MORF | Number | 3 | 0 |
| LBLMORF | Character | 64 | 0 |
| DEFMORF | Character | 255 | 0 |

### WegsegmentLktStatus

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| STATUS | code | - |
| LBLSTATUS | label | - |
| DEFSTATUS | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| STATUS | Number | 2 | 0 |
| LBLSTATUS | Character | 64 | 0 |
| DEFSTATUS | Character | 255 | 0 |

### WegsegmentLktMethode

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| METHODE | code | - |
| LBLMETHOD | label | - |
| DEFMETHOD | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| METHODE | Number | 2 | 0 |
| LBLMETHOD | Character | 64 | 0 |
| DEFMETHOD | Character | 255 | 0 |

### RijstrokenLktRichting

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| RICHTING | code | - |
| LBLRICHT | label | - |
| DEFRICHT | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| RICHTING | Number | 2 | 0 |
| LBLRICHT | Character | 64 | 0 |
| DEFRICHT | Character | 255 | 0 |

### OgkruisingLktType

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| TYPE | code | - |
| LBLTYPE | label | - |
| DEFTYPE | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| TYPE | Number | 2 | 0 |
| LBLTYPE | Character | 64 | 0 |
| DEFTYPE | Character | 255 | 0 |

### WegsegmentLktTgbep

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| TYPE | code | - |
| LBLTYPE | label | - |
| DEFTYPE | definitie | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| TYPE | Number | 2 | 0 |
| LBLTYPE | Character | 64 | 0 |
| DEFTYPE | Character | 255 | 0 |

### RltBbitgi

- Obsolete

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| ITGI_OIDN | relatieBasisbestandITGIID | N/A |
| WS_OIDN | wegsegmentID | N/A |
| TGID | TGID | N/A |
| VANPOS | vanPositie | N/A |
| TOTPOS | totPositie | N/A |
| BEGINTIJD | begintijd | N/A |
| BEGINORG | beginorganisatieCode | N/A |
| LBLBGNORG | beginorganisatieLabel | N/A |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| ITGI_OIDN | Number | 15 | 0 |
| WS_OIDN | Number | 15 | 0 |
| TGID | Character | 38 | 0 |
| VANPOS | Number | 9 | 3 |
| TOTPOS | Number | 9 | 3 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

### RltBbgrb

- Obsolete

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| GRB_OIDN | relatieBasisbestandGRBID | - |
| WS_OIDN | wegsegmentID | - |
| WVB_OIDN | identificatorWegObject | - |
| VANPOS | vanPositie | - |
| TOTPOS | totPositie | - |
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| GRB_OIDN | Number | 15 | 0 |
| WS_OIDN | Number | 15 | 0 |
| WVB_OIDN | Number | 15 | 0 |
| VANPOS | Number | 9 | 3 |
| TOTPOS | Number | 9 | 3 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

## Shapes

### Wegknoop

- ShapeGeometryType: Point

#### DBF Mapping

| DBF Column | Legacy DB Column | Event Property |
|---|---|---|
| WK_OIDN | wegknoopID | - |
| WK_UIDN | wegknoopversieID | - |
| TYPE | typeCode | - |
| LBLTYPE | typeLabel | - |
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| WK_OIDN | Number | 15 | 0 |
| WK_UIDN | Character | 18 | 0 |
| TYPE | Number | 2 | 0 |
| LBLTYPE | Character | 64 | 0 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |

### Wegsegment

- ShapeGeometryType: LineStringM

#### DBF Mapping

| DBF Column | Legacy DB Column  | Event Property |
|---|---|---|
| WS_OIDN | wegsegmentID | - |
| WS_UIDN | wegsegmentversieID | - |
| WS_GIDN | wegsegmentGeometrieversieID | - |
| B_WK_OIDN | beginWegknoopID | - |
| E_WK_OIDN | eindWegknoopID | - |
| STATUS | statusCode | - |
| LBLSTATUS | statusLabel | - |
| MORF | morfologieCode | - |
| LBLMORF | morfologieLabel | - |
| WEGCAT | categorieCode | - |
| LBLWEGCAT | categorieLabel | - |
| LSTRNMID | linksStraatnaamID | - |
| LSTRNM | straatnaamLinks | - |
| RSTRNMID | rechtsStraatnaamID | - |
| RSTRNM | straatnaamRechts | - |
| BEHEER | beheerderCode | - |
| LBLBEHEER | beheerderLabel | - |
| METHODE | methodeCode | - |
| LBLMETHOD | methodeLabel | - |            
| OPNDATUM | opnamedatum | - |
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |
| TGBEP | toegangsbeperkingCode | - |
| LBLTGBEP | toegangsbeperkingLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| WS_OIDN | Number | 15 | 0 |
| WS_UIDN | Character | 18 | 0 |
| WS_GIDN | Character | 18 | 0 |
| B_WK_OIDN | Number | 15 | 0 |
| E_WK_OIDN | Number | 15 | 0 |
| STATUS | Number | 2 | 0 |
| LBLSTATUS | Character | 64 | 0 |
| MORF | Number | 3 | 0 |
| LBLMORF | Character | 64 | 0 |
| WEGCAT | Character | 5 | 0 |
| LBLWEGCAT | Character | 64 | 0 |
| LSTRNMID | Number | 15 | 0 |
| LSTRNM | Character | 80 | 0 |
| RSTRNMID | Number | 15 | 0 |
| RSTRNM | Character | 80 | 0 |
| BEHEER | Character | 18 | 0 |
| LBLBEHEER | Character | 64 | 0 |
| METHODE | Number | 2 | 0 |
| LBLMETHOD | Character | 64 | 0 |         
| OPNDATUM | DateTime | 12 | 0 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |
| TGBEP | Number | 2 | 0 |
| LBLTGBEP | Character | 64 | 0 |

### WegsegmentLRGem

- ShapeGeometryType: LineStringM

#### DBF Mapping

| DBF Column | Legacy DB Column  | Event Property |
|---|---|---|
| WS_OIDN | wegsegmentID | - |
| WS_UIDN | wegsegmentversieID | - |
| WS_GIDN | wegsegmentGeometrieversieID | - |
| B_WK_OIDN | beginWegknoopID | - |
| E_WK_OIDN | eindWegknoopID | - |
| STATUS | statusCode | - |
| LBLSTATUS | statusLabel | - |
| MORF | morfologieCode | - |
| LBLMORF | morfologieLabel | - |
| WEGCAT | categorieCode | - |
| LBLWEGCAT | categorieLabel | - |
| LSTRNMID | linksStraatnaamID | - |
| LSTRNM | straatnaamLinks | - |
| RSTRNMID | rechtsStraatnaamID | - |
| RSTRNM | straatnaamRechts | - |
| BEHEER | beheerderCode | - |
| LBLBEHEER | beheerderLabel | - |
| METHODE | methodeCode | - |
| LBLMETHOD | methodeLabel | - |
| OPNDATUM | opnamedatum | - |
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |
| TGBEP | toegangsbeperkingCode | - |
| LBLTGBEP | toegangsbeperkingLabel | - |
| LGEM | linksGemeente | - |
| RGEM | rechtsGemeente | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| WS_OIDN | Number | 15 | 0 |
| WS_UIDN | Character | 18 | 0 |
| WS_GIDN | Character | 18 | 0 |
| B_WK_OIDN | Number | 15 | 0 |
| E_WK_OIDN | Number | 15 | 0 |
| STATUS | Number | 2 | 0 |
| LBLSTATUS | Character | 64 | 0 |
| MORF | Number | 3 | 0 |
| LBLMORF | Character | 64 | 0 |
| WEGCAT | Character | 5 | 0 |
| LBLWEGCAT | Character | 64 | 0 |
| LSTRNMID | Number | 15 | 0 |
| LSTRNM | Character | 80 | 0 |
| RSTRNMID | Number | 15 | 0 |
| RSTRNM | Character | 80 | 0 |
| BEHEER | Character | 18 | 0 |
| LBLBEHEER | Character | 64 | 0 |
| METHODE | Number | 2 | 0 |
| LBLMETHOD | Character | 64 | 0 |
| OPNDATUM | DateTime | 12 | 0 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |
| TGBEP | Number | 2 | 0 |
| LBLTGBEP | Character| 64 | 0 |
| LGEM | Number | 10 | 0 |
| RGEM | Number | 10 | 0 |

### Refpunt

- ShapeGeometryType: Point

#### DBF Mapping

| DBF Column | Legacy DB Column  | Event Property |
|---|---|---|
| RP_OIDN | referentiepuntID | - |
| RP_UIDN | referentiepuntversieID | - |
| IDENT8 | ident8 | - |
| OPSCHRIFT | opschrift | - |
| TYPE | typeCode | - |
| LBLTYPE | typeLabel | - |
| BEGINTIJD | begintijd | - |
| BEGINORG | beginorganisatieCode | - |
| LBLBGNORG | beginorganisatieLabel | - |

#### DBF Description

| Name | DataType | Length | Decimal |
|---|---|---|---|
| RP_OIDN | Number | 15 | 0 |
| RP_UIDN | Character | 18 | 0 |
| IDENT8 | Character | 8 | 0 |
| OPSCHRIFT | Number | 5 | 1 |
| TYPE | Number | 2 | 0 |
| LBLTYPE | Character | 64 | 0 |
| BEGINTIJD | DateTime | 12 | 0 |
| BEGINORG | Character | 18 | 0 |
| LBLBGNORG | Character | 64 | 0 |
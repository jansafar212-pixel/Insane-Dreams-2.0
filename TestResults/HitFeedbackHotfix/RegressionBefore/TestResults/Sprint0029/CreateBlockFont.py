"""Original AKUMU block lettering, built without external font dependencies.
Rectangular 5x7 outlines follow the existing AKUMU wordmark's A/K/U/M/2.
OpenType layout reference: https://learn.microsoft.com/en-us/typography/opentype/spec/
Run from project root. No existing game artwork is read or modified.
"""
import struct, math, sys
from pathlib import Path

patterns = {
'A':'01110/11011/11011/11111/11011/11011/11011',
'B':'11110/11011/11011/11110/11011/11011/11110',
'C':'01111/11000/11000/11000/11000/11000/01111',
'D':'11110/11011/11011/11011/11011/11011/11110',
'E':'11111/11000/11000/11110/11000/11000/11111',
'F':'11111/11000/11000/11110/11000/11000/11000',
'G':'01111/11000/11000/11011/11011/11011/01111',
'H':'11011/11011/11011/11111/11011/11011/11011',
'I':'11111/00100/00100/00100/00100/00100/11111',
'J':'00111/00011/00011/00011/11011/11011/01110',
'K':'11011/11011/11110/11100/11110/11011/11011',
'L':'11000/11000/11000/11000/11000/11000/11111',
'M':'10001/11011/10101/10101/10001/10001/10001',
'N':'11001/11101/11101/11011/11011/11011/11001',
'O':'01110/11011/11011/11011/11011/11011/01110',
'P':'11110/11011/11011/11110/11000/11000/11000',
'Q':'01110/11011/11011/11011/11111/00110/00011',
'R':'11110/11011/11011/11110/11100/11010/11011',
'S':'01111/11000/11000/01110/00011/00011/11110',
'T':'11111/00100/00100/00100/00100/00100/00100',
'U':'11011/11011/11011/11011/11011/11011/01110',
'V':'11011/11011/11011/11011/01010/01010/00100',
'W':'10001/10001/10001/10101/10101/11011/10001',
'X':'11011/11011/01010/00100/01010/11011/11011',
'Y':'11011/11011/01010/00100/00100/00100/00100',
'Z':'11111/00011/00110/01100/11000/11000/11111',
'0':'01110/11011/11111/11111/11011/11011/01110',
'1':'00100/01100/00100/00100/00100/00100/01110',
'2':'01110/11011/00011/00110/01100/11000/11111',
'3':'11110/00011/00011/01110/00011/00011/11110',
'4':'11011/11011/11011/11111/00011/00011/00011',
'5':'11111/11000/11000/11110/00011/00011/11110',
'6':'01110/11000/11000/11110/11011/11011/01110',
'7':'11111/00011/00110/00100/01100/01100/01100',
'8':'01110/11011/11011/01110/11011/11011/01110',
'9':'01110/11011/11011/01111/00011/00011/01110',
' ':'00000/00000/00000/00000/00000/00000/00000',
'.':'00000/00000/00000/00000/00000/00110/00110',
',':'00000/00000/00000/00000/00110/00110/00100',
':':'00000/00110/00110/00000/00110/00110/00000',
';':'00000/00110/00110/00000/00110/00110/00100',
'!':'00100/00100/00100/00100/00100/00000/00100',
'?':'01110/11011/00011/00110/00100/00000/00100',
'-':'00000/00000/00000/11111/00000/00000/00000',
'_':'00000/00000/00000/00000/00000/00000/11111',
'/':'00001/00011/00110/00100/01100/11000/10000',
'\\':'10000/11000/01100/00100/00110/00011/00001',
'(':'00010/00100/01000/01000/01000/00100/00010',
')':'01000/00100/00010/00010/00010/00100/01000',
'[':'01110/01000/01000/01000/01000/01000/01110',
']':'01110/00010/00010/00010/00010/00010/01110',
'+':'00000/00100/00100/11111/00100/00100/00000',
'=':'00000/00000/11111/00000/11111/00000/00000',
'<':'00010/00100/01000/10000/01000/00100/00010',
'>':'01000/00100/00010/00001/00010/00100/01000',
'"':'01010/01010/01010/00000/00000/00000/00000',
"'":'00100/00100/00100/00000/00000/00000/00000',
'$':'00100/01111/10100/01110/00101/11110/00100',
'%':'11001/11010/00010/00100/01000/01011/10011',
'#':'01010/01010/11111/01010/11111/01010/01010',
'*':'00000/10101/01110/11111/01110/10101/00000',
'|':'00100/00100/00100/00100/00100/00100/00100',
'@':'01110/10001/10111/10101/10111/10000/01111',
'^':'00100/01010/10001/00000/00000/00000/00000',
'&':'01100/10010/10100/01000/10101/10010/01101',
'~':'00000/00000/01001/10110/00000/00000/00000',
}
aliases={0xa0:' ',0xb7:'.',0x2013:'-',0x2014:'-',0x2018:"'",0x2019:"'",0x201c:'"',0x201d:'"',0x2022:'*',0x2026:'.',0x2190:'<',0x2192:'>',0x23f8:'H'}
codes=list(range(32,127))+list(aliases)
pack=lambda fmt,*v:struct.pack('>'+fmt,*v)
pad=lambda b:b+b'\0'*((-len(b))%4)
def checksum(data):return sum(struct.unpack('>'+str(len(pad(data))//4)+'I',pad(data)))&0xffffffff
def glyph(pattern):
    points=[];ends=[]
    for row,line in enumerate(pattern.split('/')):
        col=0
        while col<5:
            if line[col]!='1':col+=1;continue
            start=col
            while col<5 and line[col]=='1':col+=1
            x,y=start*100,(6-row)*100
            points.extend([(x,y),(x,y+100),(col*100,y+100),(col*100,y)])
            ends.append(len(points)-1)
    if not points:return pack('hhhhh',0,0,0,0,0)
    data=pack('hhhhh',len(ends),min(x for x,y in points),min(y for x,y in points),max(x for x,y in points),max(y for x,y in points))
    data+=pack('H'*len(ends),*ends)+pack('H',0)+bytes([1]*len(points))
    for axis in (0,1):
        previous=0
        for p in points:data+=pack('h',p[axis]-previous);previous=p[axis]
    return data

glyphs=[glyph(patterns['?'])];cmap={};widths=[650]
for code in sorted(codes):
    char=aliases.get(code,chr(code)).upper()
    if char=='`':char="'"
    if char in '{}':char='(' if char=='{' else ')'
    cmap[code]=len(glyphs);glyphs.append(glyph(patterns.get(char,patterns['?'])));widths.append(350 if char==' ' else 650)
glyf=b'';loca=[]
for g in glyphs:loca.append(len(glyf));glyf+=pad(g)
loca.append(len(glyf));count=len(glyphs)
tables={'glyf':glyf,'loca':pack('I'*len(loca),*loca),'hmtx':b''.join(pack('Hh',w,struct.unpack_from('>h',g,2)[0]) for w,g in zip(widths,glyphs))}
tables['head']=pack('IIIIHHQQhhhhHHhhh',0x10000,0x10000,0,0x5f0f3cf5,3,1000,0,0,0,0,500,700,0,7,2,1,0)
tables['hhea']=pack('IhhhH'+'h'*11+'H',0x10000,800,-200,100,650,0,150,500,1,0,0,0,0,0,0,0,count)
tables['maxp']=pack('I'+'H'*14,0x10000,count,128,32,0,0,1,0,0,0,0,0,0,0,0)
segments=sorted(cmap.items())+[(0xffff,0)]
n=len(segments);power=2**int(math.log2(n))
sub=pack('HHHHHHH',4,16+8*n,0,n*2,power*2,int(math.log2(n)),n*2-power*2)
sub+=pack('H'*n,*[c for c,g in segments])+pack('H',0)+pack('H'*n,*[c for c,g in segments])
sub+=pack('H'*n,*[(g-c)&0xffff for c,g in segments])+pack('H'*n,*([0]*n))
tables['cmap']=pack('HHHHI',0,1,3,1,12)+sub
strings={0:'Original project lettering, Kurohashi Games',1:'AKUMU Block',2:'Regular',4:'AKUMU Block Regular',6:'AKUMUBlock-Regular'}
names=b'';blob=b''
for key,value in strings.items():
    raw=value.encode('utf-16-be');names+=pack('HHHHHH',3,1,0x409,key,len(raw),len(blob));blob+=raw
tables['name']=pack('HHH',0,len(strings),6+12*len(strings))+names+blob
tables['post']=pack('IihhIIIII',0x30000,0,-100,50,1,0,0,0,0)
tables['OS/2']=pack('HhHHH'+'h'*11,0,600,400,5,0,650,650,0,75,650,650,0,350,80,300,0)+bytes([2,11,6,9,2,2,3,2,2,4])+pack('IIII',1,0,0,0)+b'KURO'+pack('HHHhhhHH',0x40,32,max(codes),800,-200,100,800,200)
n=len(tables);power=2**int(math.log2(n));header=pack('IHHHH',0x10000,n,power*16,int(math.log2(n)),n*16-power*16)
directory=b'';payload=b'';offsets={}
for tag,data in sorted(tables.items()):
    offset=12+16*n+len(payload);offsets[tag]=offset
    directory+=tag.encode()+pack('III',checksum(data),offset,len(data));payload+=pad(data)
font=bytearray(header+directory+payload)
struct.pack_into('>I',font,offsets['head']+8,(0xb1b0afba-checksum(font))&0xffffffff)
assert checksum(font)==0xb1b0afba
out=Path(sys.argv[1] if len(sys.argv)>1 else 'Assets/Art/Interface0029/AkumuBlock.ttf');out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes(font)
print(f'Wrote {out}: {len(font)} bytes, {count} original glyphs')
try:
    from PIL import ImageFont
    f=ImageFont.truetype(str(out),28);print('FreeType validation:',f.getbbox('AKUMU 2 / MEDICAL / 16/27'))
except ImportError:print('FreeType unavailable; validate via Unity font importer')

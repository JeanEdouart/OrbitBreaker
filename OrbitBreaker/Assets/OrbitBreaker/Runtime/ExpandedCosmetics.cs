using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    // Original code-native illustrations. Generated once, cached, no external IP or atlas cropping.
    internal static class ExpandedCosmetics
    {
        static readonly Dictionary<string, Sprite> Cache = new();
        static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color c); return c; }
        static readonly Color Ink = C("10203c"), Light = C("e0fcff"), Cyan = C("4ae9ed");
        static readonly Color[] TrailPalette = { C("ccdce9"), C("efa8d6"), C("65baff"), C("ffb932"), C("49e2b5"), C("ff7637"), C("69eaff"), C("a98ad8"), C("ffd76d"), C("c2f5ff"), C("7eff8b"), C("38c8b9"), C("ff9fc5"), C("fff1cd"), C("729bff") };
        static readonly string[] PlanetPalettes = {
            "348b63,718b41,cc75ac,396c68,9aae62", "309dba,516992,cf8299,244271,a4d5df",
            "aa8a54,b27448,91573f,8b9fac,c9ac77", "845544,d989b5,9ec6da,d0a25a,e1bed4",
            "9369c5,d6c2df,4cab85,486bc2,aab6ce", "31527d,7a9c67,7983a4,60a297,995d9d",
            "82bb92,4595b6,c8924c,cadbea,866ea2", "b2a6bf,e3ab9e,90b9b1,e1c59c,93a3d1",
            "504654,39324a,877d80,ab4f4f,688487", "57979b,be8687,8f77b1,729f70,467f9b",
            "7972ce,467990,986773,477b5c,946187"
        };
        public static CosmeticDefinition[] Extend(CosmeticDefinition[] current)
        {
            var list = new List<CosmeticDefinition>(current);
            Add(list,CosmeticKind.Rocket,11,"firefly|LUCIOLE|420;origami|ORIGAMI|550;sardine|SARDINE COSMIQUE|600;scarab|SCARABÉE|1200;teapot|THÉIÈRE ORBITALE|850;taxi|TAXI LUNAIRE|650;manta|MANTA|1500;vintage_comet|COMÈTE VINTAGE|750;jellyfish|MÉDUSE|1800;cactus|CACTUS|950;submarine|SOUS-MARIN|1250;geode|GÉODE|1700;space_skate|SKATE SPATIAL|1100;bee|ABEILLE|800;arctic|CAPSULE ARCTIQUE|1450;novelty|FUSÉE COQUINE|1650");
            Add(list,CosmeticKind.Rocket,27,"daily_solar|HÉLIOGRAPHE|999999;daily_crown|COURONNE AZUR|999999;daily_eclipse|ÉCLIPSE ROYALE|999999");
            Add(list,CosmeticKind.Trail,6,"moon_dust|POUSSIÈRE LUNAIRE|180;pastel|RUBAN PASTEL|400;pixels|PIXELS PERDUS|350;honey|MIEL SOLAIRE|450;polar|AURORE POLAIRE|950;forge|ÉTINCELLES DE FORGE|600;bubbles|BULLES COSMIQUES|500;ink|ENCRE STELLAIRE|650;notes|NOTES DE VOL|900;frost|CRISTAUX DE GIVRE|750;circuit|CIRCUIT IMPRIMÉ|1000;peacock|QUEUE DE PAON|1400;confetti|CONFETTIS|850;shooting_stars|ÉTOILES FILANTES|1150;sonar|ONDE SONAR|700");
            Add(list,CosmeticKind.PlanetPack,4,"garden|JARDIN CÉLESTE|1100;ocean|ARCHIPEL OCÉAN|1300;clockwork|ATELIER MÉCANIQUE|1700;candy|CONFISERIE|1500;crystalline|CRISTALLINES|2200;civilizations|CIVILISATIONS MINIATURES|2400;seasons|SAISONS|1200;paper|PAPIER DÉCOUPÉ|1600;volcanoes|VOLCANS ENDORMIS|1900;mushrooms|MONDES CHAMPIGNONS|2100;gen_z|GALAXIE GEN-Z|2600");
            Add(list,CosmeticKind.Background,4,"nebula_sea|MER DE NÉBULEUSES|800;polar_night|NUIT POLAIRE|950;star_charts|ARCHIVES STELLAIRES|1000;gold_dust|POUSSIÈRE D'OR|1300;cosmic_ocean|OCÉAN COSMIQUE|1200;distant_city|CITÉ LOINTAINE|1900;binary_dusk|CRÉPUSCULE BINAIRE|1500;monochrome|RÊVE MONOCHROME|700;ice_cathedral|CATHÉDRALE DE GLACE|1800;festival|FESTIVAL ORBITAL|1600;meme_scroll|SCROLL COSMIQUE|2300");
            Add(list,CosmeticKind.Music,0,"neon_orbit|NEON ORBIT|0;starlight|STARLIGHT|250;void_runner|VOID RUNNER|450;cosmic_disco|COSMIC DISCO|600;lunar_lounge|LUNAR LOUNGE|350;asteroid_funk|ASTEROID FUNK|700;binary_chase|BINARY CHASE|900;aurora_dream|AURORA DREAM|800;rusty_station|RUSTY STATION|500;solar_carnival|SOLAR CARNIVAL|1100;deep_blue|DEEP BLUE|550;pocket_galaxy|POCKET GALAXY|1300;meow_sad|MEOW MÉLANCOLIQUE|850");
            return list.ToArray();
        }
        static void Add(List<CosmeticDefinition> list,CosmeticKind kind,int start,string entries)
        {
            foreach(string entry in entries.Split(';')) { string[] p=entry.Split('|'); list.Add(new CosmeticDefinition(kind.ToString().ToLowerInvariant()+"_"+p[0],p[1],kind,int.Parse(p[2]),start++)); }
        }
        public static Color TrailColor(int index)
        {
            return TrailPalette[Mathf.Clamp(index-6,0,TrailPalette.Length-1)];
        }
        public static Sprite Rocket(int variant)
        {
            variant=Mathf.Clamp(variant,0,15); string key="ExpandedRocket"+variant;
            if(Cache.TryGetValue(key,out Sprite sprite))return sprite;
            var a=new Art(256,256); Color gold=C("eeb95a"), pink=C("fa8ab1"), blue=C("457de1"), green=C("49bd8c");
            // Every silhouette faces up; all appendages stay inside the canvas with a generous safe rim.
            switch(variant)
            {
                case 0: a.Ellipse(.31f,.46f,.13f,.23f,gold);a.Ellipse(.69f,.46f,.13f,.23f,gold);a.Ellipse(.5f,.5f,.2f,.33f,Ink);a.Ellipse(.5f,.56f,.12f,.15f,C("fff277"));a.Line(.38f,.77f,.3f,.87f,.02f,gold);a.Line(.62f,.77f,.7f,.87f,.02f,gold);break;
                case 1: a.Poly(Light,.5f,.91f,.12f,.23f,.5f,.36f,.88f,.23f);a.Poly(C("8db0d4"),.5f,.91f,.5f,.36f,.68f,.2f);a.Line(.5f,.88f,.31f,.28f,.012f,blue);a.Line(.5f,.88f,.68f,.25f,.012f,blue);break;
                case 2: a.Rect(.27f,.2f,.73f,.77f,C("adc3cf"));a.Ellipse(.5f,.77f,.23f,.09f,Light);a.Ellipse(.5f,.2f,.23f,.065f,C("657e91"));a.Rect(.3f,.37f,.7f,.61f,blue);a.Text("FISH",.33f,.43f,.016f,Light);a.Line(.29f,.25f,.29f,.72f,.012f,Light);a.Poly(gold,.27f,.32f,.12f,.15f,.27f,.2f);a.Poly(gold,.73f,.32f,.88f,.15f,.73f,.2f);break;
                case 3: a.Ellipse(.5f,.5f,.27f,.34f,gold);a.Ellipse(.38f,.5f,.13f,.28f,green);a.Ellipse(.62f,.5f,.13f,.28f,green);for(int i=0;i<4;i++)a.Line(.29f,.32f+i*.11f,.71f,.32f+i*.11f,.015f,Ink);a.Ellipse(.5f,.78f,.13f,.12f,Cyan);break;
                case 4: a.Ring(.72f,.49f,.16f,.21f,.045f,Light);a.Poly(Light,.32f,.49f,.15f,.75f,.24f,.78f,.45f,.59f);a.Ellipse(.49f,.43f,.25f,.23f,Light);a.Rect(.35f,.66f,.62f,.7f,blue);a.Ellipse(.49f,.74f,.045f,.045f,gold);for(int i=0;i<3;i++)a.Ellipse(.36f+i*.12f,.44f,.035f,.06f,blue);break;
                case 5: Hull(a,gold,.26f);a.Rect(.25f,.3f,.75f,.42f,Ink);for(int i=0;i<5;i++)a.Rect(.25f+i*.1f,.3f+(i%2)*.06f,.35f+i*.1f,.36f+(i%2)*.06f,Light);a.Rect(.31f,.72f,.69f,.84f,Ink);a.Text("TAXI",.33f,.75f,.027f,gold);Window(a,.5f,.57f,.13f);break;
                case 6: a.Poly(blue,.5f,.88f,.32f,.61f,.08f,.3f,.29f,.34f,.5f,.48f,.71f,.34f,.92f,.3f,.68f,.61f);a.Poly(Cyan,.5f,.81f,.4f,.49f,.5f,.24f,.6f,.49f);a.Line(.14f,.33f,.43f,.56f,.018f,Light);a.Line(.86f,.33f,.57f,.56f,.018f,Light);break;
                case 7: Hull(a,C("f3dec0"),.19f);a.Poly(C("e54c59"),.5f,.88f,.34f,.64f,.66f,.64f);a.Poly(C("e54c59"),.35f,.4f,.16f,.17f,.36f,.22f);a.Poly(C("e54c59"),.65f,.4f,.84f,.17f,.64f,.22f);Window(a,.5f,.53f,.105f);break;
                case 8: for(int i=0;i<5;i++){float x=.29f+i*.105f;a.Line(x,.43f,x+.045f,.26f,.025f,pink);a.Line(x+.045f,.26f,x-.02f,.15f,.02f,Cyan);}a.Ellipse(.5f,.57f,.29f,.23f,C("886ddd"));a.Ellipse(.45f,.65f,.18f,.09f,C("b2a6f3"));a.Ellipse(.41f,.52f,.025f,.035f,Light);a.Ellipse(.59f,.52f,.025f,.035f,Light);break;
                case 9: a.Ellipse(.5f,.53f,.14f,.3f,green);a.Line(.36f,.39f,.25f,.48f,.09f,green);a.Line(.25f,.48f,.25f,.65f,.08f,green);a.Line(.64f,.48f,.75f,.57f,.075f,green);a.Line(.75f,.57f,.75f,.72f,.07f,green);for(int i=0;i<3;i++)a.Line(.43f+i*.07f,.29f,.43f+i*.07f,.73f,.012f,C("25795c"));for(int i=0;i<5;i++)a.Ellipse(.5f+Mathf.Cos(i*1.26f)*.07f,.83f+Mathf.Sin(i*1.26f)*.06f,.055f,.05f,pink);break;
                case 10: a.Ellipse(.5f,.48f,.23f,.32f,gold);a.Rect(.58f,.73f,.63f,.88f,gold);a.Rect(.58f,.84f,.75f,.89f,gold);a.Rect(.7f,.82f,.76f,.91f,Ink);for(int i=0;i<3;i++)Window(a,.5f,.31f+i*.16f,.065f);a.Poly(blue,.29f,.37f,.14f,.24f,.29f,.24f);a.Poly(blue,.71f,.37f,.86f,.24f,.71f,.24f);break;
                case 11: a.Poly(C("333448"),.5f,.9f,.27f,.72f,.23f,.37f,.43f,.15f,.73f,.33f,.78f,.65f);a.Poly(C("c34cda"),.51f,.83f,.37f,.52f,.49f,.3f,.65f,.57f);a.Poly(C("f3a9ff"),.51f,.83f,.51f,.43f,.65f,.57f);a.Line(.48f,.3f,.41f,.2f,.018f,pink);break;
                case 12: a.Ellipse(.5f,.48f,.17f,.37f,C("db65bc"));a.Line(.4f,.19f,.4f,.77f,.016f,Cyan);a.Line(.6f,.19f,.6f,.77f,.016f,Cyan);a.Ellipse(.5f,.59f,.105f,.13f,Ink);a.Rect(.24f,.31f,.36f,.59f,C("92b8d4"));a.Rect(.64f,.31f,.76f,.59f,C("92b8d4"));Window(a,.5f,.59f,.07f);break;
                case 13: a.Ellipse(.29f,.54f,.15f,.23f,C("b3e1e8"));a.Ellipse(.71f,.54f,.15f,.23f,C("b3e1e8"));a.Ellipse(.5f,.47f,.19f,.29f,gold);for(int i=0;i<3;i++)a.Rect(.34f,.29f+i*.13f,.66f,.35f+i*.13f,Ink);a.Ellipse(.5f,.71f,.16f,.13f,gold);a.Line(.42f,.79f,.35f,.87f,.015f,Ink);a.Line(.58f,.79f,.65f,.87f,.015f,Ink);a.Ellipse(.44f,.74f,.022f,.03f,Ink);a.Ellipse(.56f,.74f,.022f,.03f,Ink);break;
                case 14: Hull(a,Light,.23f);a.Poly(blue,.27f,.4f,.16f,.19f,.31f,.23f);a.Poly(blue,.73f,.4f,.84f,.19f,.69f,.23f);a.Rect(.28f,.3f,.72f,.36f,C("f99649"));Window(a,.5f,.58f,.13f);a.Line(.4f,.58f,.53f,.68f,.012f,Light);break;
                default: a.Ellipse(.34f,.28f,.17f,.14f,pink);a.Ellipse(.66f,.28f,.17f,.14f,pink);a.Rect(.36f,.29f,.64f,.7f,pink);a.Ellipse(.5f,.7f,.14f,.17f,C("ffa5c9"));a.Line(.39f,.67f,.61f,.67f,.013f,C("cf618f"));Window(a,.5f,.48f,.075f);a.Line(.4f,.36f,.4f,.6f,.013f,C("ffe4f0"));break;
            }
            return Cache[key]=a.Sprite(key);
        }
        static void Hull(Art a,Color c,float w){a.Ellipse(.5f,.52f,w,.35f,c);a.Rect(.5f-w*.65f,.17f,.5f+w*.65f,.25f,Ink);a.Line(.5f-w*.6f,.35f,.5f-w*.6f,.65f,.013f,Light);}
        static void Window(Art a,float x,float y,float r){a.Ellipse(x,y,r,r,C("bdcbd4"));a.Ellipse(x,y,r*.75f,r*.75f,Ink);a.Ellipse(x-r*.16f,y+r*.14f,r*.5f,r*.5f,Cyan);a.Ellipse(x-r*.26f,y+r*.3f,r*.16f,r*.16f,Light);}

        public static Sprite Planet(int pack,int sequence)
        {
            pack=Mathf.Clamp(pack,0,10);int v=Mathf.Abs(sequence%5);string key="ExpandedPlanet"+pack+"_"+v;if(Cache.TryGetValue(key,out Sprite s))return s;
            var a=new Art(256,256);Color basis=C(PlanetPalettes[pack].Split(',')[v]);
            a.Sphere(basis,pack*7+v);
            var rng=new System.Random(pack*193+v*751+67);
            for(int i=0;i<22;i++)
            {
                float x=.23f+(float)rng.NextDouble()*.54f,y=.22f+(float)rng.NextDouble()*.56f;if(new Vector2(x-.5f,y-.5f).magnitude>.32f)continue;
                Color c=Color.Lerp(basis,Light,.22f+(float)rng.NextDouble()*.4f);float r=.018f+(float)rng.NextDouble()*.035f;
                switch(pack)
                {
                    case 0:a.Ellipse(x,y,r*1.2f,r, C(v==2?"ec98c8":"90c875"));a.Line(x,y-r,x,y+r,.004f,C("cceea5"));break;
                    case 1:a.Ellipse(x,y,r*1.5f,r*.7f,C(v==4?"dceef1":"9ccf90"));a.Line(x-r,y+r,x+r,y+r,.005f,C("7ae9e8"));break;
                    case 2:a.Ring(x,y,r,r,.008f,c);for(int z=0;z<4;z++)a.Line(x-r,y+z*r*.5f,x+r,y+z*r*.5f,.003f,Ink);break;
                    case 3:a.Ellipse(x,y,r,r,C("f2d5b1"));a.Line(x-r*.4f,y-r*.5f,x+r*.4f,y+r*.5f,.008f,C("f375a9"));break;
                    case 4:a.Poly(c,x,y+r*1.5f,x-r,y,x,y-r,x+r,y);a.Line(x,y+r,x,y-r,.004f,Light);break;
                    case 5:a.Rect(x-r,y-r,x+r,y+r*2,c);for(int z=0;z<3;z++)a.Rect(x-r*.5f,y+z*r*.45f,x-r*.1f,y+z*r*.45f+.008f,Cyan);break;
                    case 6:a.Ellipse(x,y,r*1.5f,r*.6f,C(v==2?"ec9c51":v==3?"f0f8ff":"ccdeb0"));break;
                    case 7:a.Poly(c,x-r,y-r,x-r*.8f,y+r,x+r,y+r*.6f,x+r*.7f,y-r*.5f);break;
                    case 8:a.Line(x-r,y+r,x,y,.008f,C("f3824f"));a.Line(x,y,x+r*.6f,y-r,.005f,C("ffcc64"));break;
                    case 9:a.Rect(x-r*.16f,y-r,x+r*.16f,y+r*.4f,C("ded4b1"));a.Ellipse(x,y+r*.3f,r,r*.55f,C(v%2==0?"71e8da":"f1a7b8"));a.Ellipse(x-r*.3f,y+r*.4f,r*.15f,r*.12f,Light);break;
                }
            }
            if(pack==10)
            {
                if(v==0){a.Text("67",.24f,.34f,.075f,C("ffff8f"));a.Line(.25f,.3f,.76f,.3f,.014f,Cyan);}
                if(v==1){a.Rect(.35f,.27f,.65f,.77f,C("ae7948"));a.Ellipse(.5f,.77f,.15f,.055f,C("e6b778"));for(int j=0;j<5;j++)a.Line(.37f+j*.06f,.3f,.38f+j*.06f,.7f,.006f,C("6b492f"));a.Ellipse(.43f,.61f,.04f,.05f,Light);a.Ellipse(.57f,.61f,.04f,.05f,Light);a.Ellipse(.43f,.61f,.019f,.028f,Ink);a.Ellipse(.57f,.61f,.019f,.028f,Ink);a.Line(.43f,.43f,.57f,.43f,.02f,Ink);}
                if(v==2){a.Ellipse(.51f,.52f,.19f,.14f,C("e9a743"));a.Ellipse(.41f,.61f,.13f,.1f,C("eda648"));a.Line(.58f,.43f,.68f,.3f,.055f,Light);a.Ellipse(.69f,.3f,.06f,.055f,Light);for(int j=0;j<12;j++)a.Ellipse(.38f+(j%4)*.07f,.48f+(j/4)*.05f,.016f,.015f,C("ab662c"));}
                if(v==3){a.Text("NPC",.24f,.4f,.048f,Cyan);a.Rect(.3f,.3f,.7f,.34f,Ink);a.Rect(.3f,.3f,.56f,.34f,C("a5ff7a"));}
                if(v==4){a.Text("BRUH",.2f,.4f,.04f,Light);a.Ring(.5f,.5f,.34f,.34f,.014f,C("ff92bf"));}
            }
            a.ClipSphere();
            return Cache[key]=a.Sprite(key);
        }

        public static Sprite Background(int variant)
        {
            variant=Mathf.Clamp(variant,0,10);string key="ExpandedBackground"+variant;if(Cache.TryGetValue(key,out Sprite s))return s;
            var a=new Art(384,768);string[] hues={"265a70","174b50","283c67","584331","21355f","26384d","64384f","283445","294d68","4c285a","382956"};Color tint=C(hues[variant]), night=C("040b19");
            // Periodic noise and motifs keep vertical wrap seamless in the existing scrolling layers.
            for(int y=0;y<a.H;y++)for(int x=0;x<a.W;x++){float u=(float)x/a.W,v=(float)y/a.H;float n=(Mathf.Sin(u*16+Mathf.Sin(v*Mathf.PI*2)*3)+Mathf.Cos(u*9-v*Mathf.PI*4)+2)*.25f;float edge=.3f+.7f*Mathf.Abs(u-.5f)*2;float strength=.08f+n*n*.34f*edge;if(variant==1)strength+=Mathf.Pow(Mathf.Max(0,Mathf.Sin(u*18+Mathf.Sin(v*Mathf.PI*2)*4)),18)*.18f;if(variant==4)strength+=Mathf.Pow(Mathf.Max(0,Mathf.Cos(v*Mathf.PI*16+u*8)),24)*.08f;a.Pixel(x,y,Color.Lerp(night,tint,strength));}
            var rng=new System.Random(variant*1057+18);for(int i=0;i<160;i++){float x=(float)rng.NextDouble(),y=.01f+(float)rng.NextDouble()*.98f,r=.0009f+(float)rng.NextDouble()*.002f;Color col=Color.Lerp(tint,Light,.3f+(float)rng.NextDouble()*.25f);a.Ellipse(x,y,r,r*.5f,col);if(i%14==0){a.Line(x-r*2,y,x+r*2,y,.001f,col);a.Line(x,y-r,x,y+r,.001f,col);}}
            for(int i=0;i<8;i++){float x=i%2==0 ? .1f : .9f,y=.07f+i*.12f;Color dim=Color.Lerp(C("040b19"),tint,.6f);
                if(variant==2){a.Line(x,y,.5f,y+.045f,.0015f,dim);a.Ring(x,y,.035f,.018f,.0015f,dim);}
                if(variant==5){a.Rect(x-.04f,y-.012f,x+.04f,y+.012f,dim);a.Rect(x-.007f,y-.045f,x+.007f,y+.045f,dim);a.Line(x-.09f,y-.03f,x+.09f,y+.03f,.004f,dim);}
                if(variant==6){a.Ellipse(x,y,.09f,.045f,dim);a.Ring(x,y,.13f,.065f,.006f,dim);}
                if(variant==8)a.Poly(dim,x,y+.065f,x-.065f,y,x,y-.065f,x+.065f,y);
                if(variant==9){a.Ellipse(x,y,.025f,.02f,dim);a.Line(x,y-.02f,x,y-.045f,.002f,dim);}
                if(variant==10){string[] words={"67","BRUH","NPC","GG","LOL","AFK","67","WOW"};a.Text(words[i],x<.5f ? .015f : .79f,y,.011f,Color.Lerp(dim,C("b198d7"),.18f));a.Ring(x,y+.04f,.027f,.015f,.002f,dim);}
            }
            return Cache[key]=a.Sprite(key,true);
        }

        public static Sprite TrailSprite(int index)
        {
            int v=Mathf.Clamp(index-6,0,14);string key="TrailMotif"+v;if(Cache.TryGetValue(key,out Sprite s))return s;var a=new Art(64,64);Color w=Color.white;
            switch(v){case 0:a.Ellipse(.5f,.5f,.12f,.12f,w);break;case 1:a.Line(.2f,.3f,.8f,.7f,.12f,w);break;case 2:a.Rect(.25f,.25f,.75f,.75f,w);break;case 3:a.Ellipse(.5f,.43f,.24f,.23f,w);a.Poly(w,.5f,.83f,.28f,.45f,.72f,.45f);break;case 4:a.Poly(w,.16f,.25f,.34f,.72f,.52f,.44f,.8f,.8f,.67f,.26f,.49f,.5f);break;case 5:a.Poly(w,.5f,.9f,.4f,.5f,.5f,.1f,.6f,.5f);break;case 6:a.Ring(.5f,.5f,.3f,.3f,.035f,w);break;case 7:a.Ellipse(.5f,.5f,.31f,.18f,new Color(1,1,1,.45f));a.Ellipse(.39f,.6f,.19f,.12f,w);break;case 8:a.Ellipse(.35f,.3f,.15f,.12f,w);a.Line(.48f,.3f,.48f,.8f,.055f,w);a.Line(.48f,.8f,.75f,.68f,.05f,w);break;case 9:for(int z=0;z<3;z++){float t=z*Mathf.PI/3;a.Line(.5f-Mathf.Cos(t)*.34f,.5f-Mathf.Sin(t)*.34f,.5f+Mathf.Cos(t)*.34f,.5f+Mathf.Sin(t)*.34f,.04f,w);}break;case 10:a.Line(.2f,.2f,.5f,.2f,.035f,w);a.Line(.5f,.2f,.5f,.7f,.035f,w);a.Line(.5f,.7f,.8f,.7f,.035f,w);a.Ellipse(.2f,.2f,.07f,.07f,w);break;case 11:a.Ring(.5f,.5f,.22f,.33f,.07f,w);a.Ellipse(.5f,.5f,.07f,.13f,w);break;case 12:a.Poly(w,.2f,.2f,.5f,.8f,.8f,.3f);break;case 13:a.Poly(w,.5f,.9f,.6f,.6f,.9f,.5f,.6f,.4f,.5f,.1f,.4f,.4f,.1f,.5f,.4f,.6f);break;default:a.Ring(.5f,.5f,.32f,.32f,.035f,w);a.Ring(.5f,.5f,.2f,.2f,.025f,w);break;}
            return Cache[key]=a.Sprite(key);
        }

        sealed class Art
        {
            public readonly int W,H;readonly Color32[] pixels;
            public Art(int w,int h){W=w;H=h;pixels=new Color32[w*h];}
            public void Pixel(int x,int y,Color c){if(x>=0&&y>=0&&x<W&&y<H)pixels[y*W+x]=c;}
            public void ClipSphere(){for(int y=0;y<H;y++)for(int x=0;x<W;x++){float dx=x/(float)W-.5f,dy=y/(float)H-.5f;if(dx*dx+dy*dy>.16f)pixels[y*W+x]=new Color32(0,0,0,0);}}
            public void Rect(float x0,float y0,float x1,float y1,Color c){for(int y=Mathf.Max(0,(int)(y0*H));y<Mathf.Min(H,(int)(y1*H));y++)for(int x=Mathf.Max(0,(int)(x0*W));x<Mathf.Min(W,(int)(x1*W));x++)Pixel(x,y,c);}
            public void Ellipse(float cx,float cy,float rx,float ry,Color c){for(int y=Mathf.Max(0,(int)((cy-ry)*H));y<Mathf.Min(H,(int)((cy+ry)*H)+1);y++)for(int x=Mathf.Max(0,(int)((cx-rx)*W));x<Mathf.Min(W,(int)((cx+rx)*W)+1);x++){float dx=(x/(float)W-cx)/rx,dy=(y/(float)H-cy)/ry;if(dx*dx+dy*dy<=1)Pixel(x,y,c);}}
            public void Ring(float cx,float cy,float rx,float ry,float width,Color c){for(int y=Mathf.Max(0,(int)((cy-ry)*H));y<Mathf.Min(H,(int)((cy+ry)*H)+1);y++)for(int x=Mathf.Max(0,(int)((cx-rx)*W));x<Mathf.Min(W,(int)((cx+rx)*W)+1);x++){float dx=(x/(float)W-cx)/rx,dy=(y/(float)H-cy)/ry,d=Mathf.Sqrt(dx*dx+dy*dy);if(d<=1&&d>=1-width/rx)Pixel(x,y,c);}}
            public void Line(float x0,float y0,float x1,float y1,float width,Color c){int n=Mathf.Max(2,Mathf.CeilToInt(Vector2.Distance(new Vector2(x0*W,y0*H),new Vector2(x1*W,y1*H))));for(int i=0;i<=n;i++)Ellipse(Mathf.Lerp(x0,x1,i/(float)n),Mathf.Lerp(y0,y1,i/(float)n),width*.5f,width*W/H*.5f,c);}
            public void Poly(Color c,params float[] p){for(int y=0;y<H;y++)for(int x=0;x<W;x++){float px=x/(float)W,py=y/(float)H;bool inside=false;for(int i=0,j=p.Length-2;i<p.Length;j=i,i+=2)if((p[i+1]>py)!=(p[j+1]>py)&&px<(p[j]-p[i])*(py-p[i+1])/(p[j+1]-p[i+1])+p[i])inside=!inside;if(inside)Pixel(x,y,c);}}
            public void Sphere(Color c,int seed){for(int y=0;y<H;y++)for(int x=0;x<W;x++){float dx=(x/(float)W-.5f)/.4f,dy=(y/(float)H-.5f)/.4f,r=dx*dx+dy*dy;if(r>1)continue;float z=Mathf.Sqrt(1-r);float shade=Mathf.Clamp01(.35f+z*.5f-dx*.19f+dy*.16f);float noise=Mathf.PerlinNoise(x*.027f+seed,y*.027f)*.14f;Color col=Color.Lerp(Ink,c,shade+noise);if(r>.94f)col=Color.Lerp(col,Cyan,.22f);Pixel(x,y,col);}}
            public void Text(string text,float x,float y,float size,Color c){foreach(char ch in text){string bits=ch switch{'6'=>"111100111101111",'7'=>"111001010010010",'B'=>"110101110101110",'R'=>"110101110101101",'U'=>"101101101101111",'H'=>"101101111101101",'N'=>"101111111111101",'P'=>"110101110100100",'C'=>"111100100100111",'G'=>"111100101101111",'L'=>"100100100100111",'O'=>"111101101101111",'A'=>"010101111101101",'F'=>"111100110100100",'K'=>"101101110101101",'W'=>"101101111111101",'T'=>"111010010010010",'X'=>"101101010101101",'I'=>"111010010010111",'S'=>"111100111001111",_=>"000000000000000"};for(int r=0;r<5;r++)for(int col=0;col<3;col++)if(bits[r*3+col]=='1')Rect(x+col*size,y+(4-r)*size,x+(col+.84f)*size,y+(4-r+.84f)*size,c);x+=size*4;}}
            public Sprite Sprite(string name,bool background=false){var t=new Texture2D(W,H,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear,wrapMode=background?TextureWrapMode.Repeat:TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};t.SetPixels32(pixels);t.Apply(false,true);var s=UnityEngine.Sprite.Create(t,new Rect(0,0,W,H),Vector2.one*.5f,H,0,SpriteMeshType.FullRect);s.name=name;return s;}
        }
    }
}

// Deterministic original compositions. No downloaded samples.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const output=process.argv[2];if(!output)throw Error('Pass the OrbitMusic output directory');
const sr=44100;
const tracks=[
 ['neon_orbit',132,[48,45,41,43],[76,79,83,79,74,76,79,71,72,76,79,76,69,72,76,67]],
 ['starlight',116,[48,53,57,55],[72,76,79,83,79,76,74,79,76,81,84,81,79,76,74,71]],
 ['void_runner',148,[45,40,41,43],[69,72,76,69,68,71,76,71,65,69,72,76,67,71,74,68]],
 ['cosmic_disco',124,[50,43,48,45],[74,77,81,84,81,77,79,81,72,76,79,83,79,76,77,72]],
 ['lunar_lounge',108,[48,53,50,55],[72,-1,76,79,-1,76,74,-1,71,74,-1,79,76,-1,74,72]],
 ['asteroid_funk',126,[43,48,46,50],[67,-1,70,72,-1,74,72,70,67,79,-1,77,74,-1,72,70]],
 ['binary_chase',152,[45,41,48,40],[69,81,76,72,71,83,76,-1,69,81,72,76,68,80,71,76]],
 ['aurora_dream',118,[50,55,47,52],[74,81,78,-1,76,83,81,-1,78,85,81,78,76,-1,73,-1]],
 ['rusty_station',112,[40,43,38,41],[64,-1,-1,71,67,-1,66,-1,62,-1,69,-1,65,64,-1,59]],
 ['solar_carnival',136,[53,58,55,60],[77,81,84,-1,82,81,79,77,79,82,86,-1,84,82,81,79]],
 ['deep_blue',100,[38,34,41,36],[62,-1,-1,69,-1,-1,65,-1,60,-1,67,-1,-1,65,62,-1]],
 ['pocket_galaxy',128,[48,55,57,53],[72,76,79,76,74,-1,71,74,76,79,81,79,77,76,74,-1]],
 ['cyberpunk',140,[43,38,39,41],[67,70,74,79,74,70,67,74,79,82,79,74,70,67,63,-1]]];
const frac=x=>x-Math.floor(x),freq=n=>440*Math.pow(2,(n-69)/12),sq=p=>Math.sin(2*Math.PI*p)>=0?1:-1,tri=p=>1-4*Math.abs(frac(p)-.5);
function old(i,id,track){
 const [,bpm,roots,melody]=track,t=i/sr,b=t/(60/bpm),beat=Math.floor(b),p=frac(b),half=Math.floor(b*2),hp=frac(b*2),rf=freq(roots[Math.floor(beat/8)%4]),nf=freq(melody[half%16]),env=Math.exp(-hp*4);
 let bass=sq(rf*t)*.065*Math.exp(-p*3.2),lead=sq(nf*t)*.045*env,sparkle=Math.sin(2*Math.PI*nf*2*t)*.018*env,kick=Math.sin(2*Math.PI*(105-57*p)*t)*Math.exp(-p*13)*.13;
 const noise=frac(Math.sin(i*12.9898)*43758.5453/2)*2-1;
 let hat=noise*Math.exp(-hp*26)*.025,snare=beat%4===1||beat%4===3?noise*Math.exp(-p*18)*.055:0;
 if(id===1){lead*=.55;sparkle*=1.8;kick*=.65;hat*=.6;}if(id===2){bass*=1.45;sparkle*=.35;snare*=1.25;}if(id===3){lead*=half%4===0?.4:1.2;bass*=beat%2===0?1.4:.8;hat*=1.4;}
 return bass+lead+sparkle+kick+hat+snare;
}
function original(i,id,track){
 const [,bpm,roots,melody]=track,spb=60/bpm,t=i/sr,b=t/spb,beat=Math.floor(b),bar=Math.floor(b/4),step=Math.floor(b*4),eighth=Math.floor(b*2),p=frac(b),ep=frac(b*2),six=frac(b*4),root=roots[Math.floor(bar/2)%4],section=Math.floor(bar/4);
 const note=melody[(eighth+(section===2?8:0))%16],bridge=section===2,funk=id===5,chase=id===6,dream=id===7,rusty=id===8,carnival=id===9,deep=id===10;
 let seed=(Math.imul(i+1,1664525)+Math.imul(id+1,1013904223))>>>0;seed^=seed>>>13;seed=Math.imul(seed,1274126177);const noise=(seed>>>0)/4294967295*2-1;
 const gate=Math.min(1,ep*45)*Math.exp(-ep*(deep?3:5));let lead=0;
 if(note>=0){const ph=freq(note+(bridge&&chase?12:0))*ep*spb/2;lead=(dream?tri(ph):rusty?Math.sin(2*Math.PI*ph+1.1*Math.sin(2*Math.PI*ph*2.01)):sq(ph))*.032*gate;if(dream||deep)lead+=Math.sin(2*Math.PI*ph*2)*.018*gate;}
 const pattern=funk?[0,-1,0,12,-1,7,0,-1]:carnival?[0,7,12,7,0,7,12,7]:[0,0,7,0,0,12,7,0],interval=pattern[eighth%8],bp=funk||carnival?ep:p;
 const bass=interval<0?0:tri(freq(root+interval)*bp*spb/(funk||carnival?2:1))*.065*Math.min(1,bp*60)*Math.exp(-bp*3.1);
 const chord=dream?[0,7,14,19]:chase?[0,3,7,12]:[0,4,7,11],arp=bridge||dream||chase?tri(freq(root+24+chord[step%4])*six*spb/4)*.016*Math.exp(-six*6)*Math.min(1,six*30):0;
 const kickOn=deep?beat%4===0:funk?(step%16===0||step%16===6||step%16===10):beat%2===0||carnival||chase,kp=funk?six:p,kt=kp*spb/(funk?4:1),kick=kickOn?Math.sin(2*Math.PI*(48*kt+1.5*(1-Math.exp(-kt*40))))*.10*Math.exp(-kt*22):0;
 const snare=beat%4===1||beat%4===3?noise*.04*Math.exp(-p*(rusty?32:22)):0,hat=noise*(deep?.009:funk?.024:.014)*Math.exp(-(chase?six:ep)*35),bell=rusty&&step%8===0?Math.sin(2*Math.PI*freq(root+36)*six*spb/4)*Math.sin(2*Math.PI*freq(root+43)*six*spb/4)*.028*Math.exp(-six*8):0;
 return lead+bass+arp+kick+snare+hat+bell;
}
function write(track,id){
 const frames=Math.ceil(64*60/track[1]*sr),samples=new Float32Array(frames);let sum=0,peak=0;
 for(let i=0;i<frames;i++){let x=(id<4?old(i,id,track):original(i,id,track))*Math.min(1,i/(sr*.004),(frames-1-i)/(sr*.004));samples[i]=x;sum+=x*x;peak=Math.max(peak,Math.abs(x));}
 const gain=id<4?1:Math.min(.036/Math.sqrt(sum/frames),.7/peak),wav=Buffer.alloc(44+frames*2);
 wav.write('RIFF');wav.writeUInt32LE(wav.length-8,4);wav.write('WAVEfmt ',8);wav.writeUInt32LE(16,16);wav.writeUInt16LE(1,20);wav.writeUInt16LE(1,22);wav.writeUInt32LE(sr,24);wav.writeUInt32LE(sr*2,28);wav.writeUInt16LE(2,32);wav.writeUInt16LE(16,34);wav.write('data',36);wav.writeUInt32LE(frames*2,40);
 for(let i=0;i<frames;i++){if(!Number.isFinite(samples[i]))throw Error('Invalid sample');wav.writeInt16LE(Math.round(Math.max(-1,Math.min(1,samples[i]*gain))*32767),44+i*2);}
 const file=path.join(output,track[0]+'.wav');fs.writeFileSync(file,wav);
 const guid=crypto.createHash('md5').update('orbit-breaker-original-music-'+track[0]).digest('hex');
 if(!fs.existsSync(file+'.meta'))fs.writeFileSync(file+'.meta',`fileFormatVersion: 2\nguid: ${guid}\nAudioImporter:\n  externalObjects: {}\n  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 0\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n    compressionFormat: 1\n    quality: 0.8\n    conversionMode: 0\n    preloadAudioData: 0\n  platformSettingOverrides: {}\n  forceToMono: 1\n  normalize: 0\n  ambisonic: 0\n  loadInBackground: 1\n  userData: Original offline composition - no external samples\n  assetBundleName: \n  assetBundleVariant: \n`);
 return {id,name:track[0],bpm:track[1],seconds:frames/sr,peak:peak*gain,rms:Math.sqrt(sum/frames)*gain,first:samples[0],last:samples[frames-1],bytes:wav.length};
}
fs.mkdirSync(output,{recursive:true});console.log(JSON.stringify(tracks.map(write),null,2));

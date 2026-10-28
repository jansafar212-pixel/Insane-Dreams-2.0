from pathlib import Path
p=Path('Assets/Scripts/Combat/AttackExecutor.cs');s=p.read_text()
s=s.replace('        private float nextEmptyCue;','''        private float nextEmptyCue;
        private RuntimeWeapon bloomWeapon;
        private float bloom,lastSpreadShot;
        public Material TracerMaterial=>trails!=null&&trails.Length>0?trails[0].sharedMaterial:null;
        public Vector2 PreviewEnd(Vector2 direction,float range)
        {
            Vector2 start=Origin;var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(hitLayers.value|physicalImpactLayers.value);
            int count=Physics2D.Raycast(start,direction,filter,hits,range);float nearest=range;
            for(int i=0;i<count;i++)if(!hits[i].collider.transform.IsChildOf(transform)&&!InsaneDreams.Enemies.SamuraiBodyHitbox.IsMovementShape(hits[i].collider))nearest=Mathf.Min(nearest,hits[i].distance);
            return start+direction*nearest;
        }''')
start=s.index('            var filter = new ContactFilter2D',s.index('public bool Attack('))
end=s.index('        private void Update() => FadeTrails();',start)
old=s[start:end]
body=old[:old.index('            ShotsFired++;')]
body=body.replace('weapon.Definition.Damage, owner.Team','weapon.Definition.DamageAtDistance(distance), owner.Team')
body=body.replace('corpse.ReceiveRegionDamage(weapon.Definition.Damage,impact)','corpse.ReceiveRegionDamage(weapon.Definition.DamageAtDistance(distance),impact)')
tail=old[old.index('            LastShotOrigin = origin;'):]
tail=tail.replace('            Fired?.Invoke(direction, end, damaged);\n','')
tail=tail.replace('Color color=owner.Team==CombatTeam.Enemy?enemyTracer:friendlyTracer;','Color color=weapon.Definition.CustomTracer?weapon.Definition.TracerColor:owner.Team==CombatTeam.Enemy?enemyTracer:friendlyTracer;')
tail=tail.replace('tracerWidth*Random.Range','tracerWidth*weapon.Definition.TracerScale*Random.Range')
tail=tail.replace('            return true;','            return damaged;')
new='''            if(bloomWeapon!=weapon){bloomWeapon=weapon;bloom=0;}
            bloom=Mathf.MoveTowards(bloom,0,Mathf.Max(0,now-lastSpreadShot)*weapon.Definition.MaximumBloom*1.5f);
            float spread=weapon.Definition.SpreadDegrees+(weapon.Definition.Pellets==1?bloom:0);
            if(weapon.Definition.Pellets==1&&spread>0)direction=Quaternion.Euler(0,0,Random.Range(-spread,spread))*direction;
            bool damaged=TracePellet(weapon,origin,direction);
            var centerEnd=LastShotEnd;var centerNormal=LastHitNormal;var centerMaterial=LastImpactMaterial;var centerSurface=LastSurface;
            for(int i=1;i<weapon.Definition.Pellets;i++)
            {
                int slot=(i+1)/2*(i%2==1?1:-1);
                float angle=slot*spread/Mathf.Max(1,weapon.Definition.Pellets-1);
                damaged|=TracePellet(weapon,origin,Quaternion.Euler(0,0,angle)*direction);
            }
            bloom=Mathf.Min(weapon.Definition.MaximumBloom,bloom+weapon.Definition.BloomPerShot);lastSpreadShot=now;
            LastShotOrigin=origin;LastShotDirection=direction;LastShotEnd=centerEnd;LastHitNormal=centerNormal;LastImpactMaterial=centerMaterial;LastSurface=centerSurface;
            ShotsFired++;Fired?.Invoke(direction,centerEnd,damaged);
            return true;
        }
        private bool TracePellet(RuntimeWeapon weapon,Vector2 origin,Vector2 direction)
        {
'''+body+tail
s=s[:start]+new+s[end:];p.write_text(s)

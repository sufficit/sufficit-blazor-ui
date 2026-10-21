const connections=new WeakMap();
export function connect(trigger,panel) {
 const items=()=>[...panel.querySelectorAll('button:not(:disabled),a[href]')];
 const position=()=>{const r=trigger.getBoundingClientRect();const p=panel.getBoundingClientRect();panel.style.left=Math.max(8,Math.min(r.right-p.width,innerWidth-p.width-8))+'px';panel.style.top=Math.max(8,Math.min(r.bottom+4,innerHeight-p.height-8))+'px';};
 const toggle=e=>{if(e.newState==='open'){position();items().forEach(b=>b.setAttribute('role','menuitem'));items()[0]?.focus();}};
 const key=e=>{const all=items();const i=all.indexOf(document.activeElement);if(['ArrowDown','ArrowUp','Home','End'].includes(e.key)){e.preventDefault();all[e.key==='Home'?0:e.key==='End'?all.length-1:(i+(e.key==='ArrowUp'?-1:1)+all.length)%all.length]?.focus();}};
 const click=e=>{if(e.target.closest('button,a[href]')){panel.hidePopover();trigger.focus();}};
 panel.addEventListener('toggle',toggle);panel.addEventListener('keydown',key);panel.addEventListener('click',click);
 const resize=()=>{if(panel.matches(':popover-open'))position();};
 window.addEventListener('resize',resize);
 connections.set(panel,()=>{panel.removeEventListener('toggle',toggle);panel.removeEventListener('keydown',key);panel.removeEventListener('click',click);window.removeEventListener('resize',resize);});
}
export function disconnect(panel){connections.get(panel)?.();connections.delete(panel);}

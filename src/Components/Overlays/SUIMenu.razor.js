// Stable IDs allow disposal after Blazor has removed the element reference.
const connections=new Map();
export function connect(trigger,panel) {
 // Blazor can remove the render tree before this queued interop call arrives.
 if(!trigger?.isConnected || !panel?.isConnected || !panel.id)return;
 // Reconnection must not duplicate document/window listeners.
 disconnect(panel);
 const items=()=>[...panel.querySelectorAll('button:not(:disabled),a[href]')];
 const position=()=>{const r=trigger.getBoundingClientRect();const p=panel.getBoundingClientRect();panel.style.left=Math.max(8,Math.min(r.right-p.width,innerWidth-p.width-8))+'px';panel.style.top=Math.max(8,Math.min(r.bottom+4,innerHeight-p.height-8))+'px';};
 const toggle=e=>{if(e.newState==='open'){position();items().forEach(b=>b.setAttribute('role','menuitem'));items()[0]?.focus();}};
 const key=e=>{const all=items();const i=all.indexOf(document.activeElement);if(['ArrowDown','ArrowUp','Home','End'].includes(e.key)){e.preventDefault();all[e.key==='Home'?0:e.key==='End'?all.length-1:(i+(e.key==='ArrowUp'?-1:1)+all.length)%all.length]?.focus();}};
 const click=e=>{if(e.target.closest('button,a[href]')){panel.hidePopover();trigger.focus();}};
 panel.addEventListener('toggle',toggle);panel.addEventListener('keydown',key);panel.addEventListener('click',click);
 const resize=()=>{if(panel.matches(':popover-open'))position();};
 // The panel is fixed to the viewport: follow the trigger on any scroll (capture covers
 // inner scroll containers) and close once the trigger leaves the visible area.
 const scroll=e=>{if(!panel.matches(':popover-open')||panel.contains(e.target))return;const r=trigger.getBoundingClientRect();if(r.bottom<0||r.top>innerHeight){panel.hidePopover();return;}position();};
 window.addEventListener('resize',resize);
 document.addEventListener('scroll',scroll,{capture:true,passive:true});
 connections.set(panel.id,()=>{panel.removeEventListener('toggle',toggle);panel.removeEventListener('keydown',key);panel.removeEventListener('click',click);window.removeEventListener('resize',resize);document.removeEventListener('scroll',scroll,{capture:true});});
}
export function disconnect(panel){
 const id=typeof panel==='string'?panel:panel?.id;
 connections.get(id)?.();connections.delete(id);
}

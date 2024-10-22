using System;
using UnityEngine;

public interface ICrashLander
{
    public void CrashLand(Entity entity, Action action){
        if (BoardRender.Instance != null && !BoardRender.Instance.IsInitializedEntity(entity.Position, Board.Instance.TurnNumberOf(Entity.BoardState))){
            BoardRender.Instance.AddInitializedEntity(entity.Position, Board.Instance.TurnNumberOf(Entity.BoardState));
            BoardRender.Instance.ReRenderEntity(entity).AddOverrideAnimation(CrashAnimation(entity.EntityEnum, action));
        }
    }
    private Func<UniversalRenderer, float, float> CrashAnimation(EntityEnum entityEnum, Action action){
        return (universalRenderer, counter) => _CrashAnimation(universalRenderer, entityEnum, counter, action);
    }
    
    private float _CrashAnimation(UniversalRenderer universalRenderer, EntityEnum entityEnum, float counter, Action action){
        int frame = (int)(counter * 60);
        counter += Time.deltaTime;
        CoreAnimator coreAnimator = universalRenderer.GetCoreAnimator();
        coreAnimator.SetSpriteOutline(false);
        coreAnimator.enabled = false;
        int maxFrame = SpriteManager.GetAnimation(entityEnum, AnimE.Crash).Length;
        if (frame < maxFrame){
            coreAnimator.SampleAnim(AnimE.Crash, frame);
            return counter;
        }
        action();
        coreAnimator.PlayAnim(AnimE.Idle);
        coreAnimator.SampleAnim(AnimE.Idle, 0);
        coreAnimator.enabled = true;
        return -1;
    }
}

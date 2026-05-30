using System;
using System.Threading.Tasks;

namespace MapSystem.Effects
{
    /// <summary>
    /// 맵 전환 효과 인터페이스
    /// 다양한 전환 효과(페이드, 와이프 등)를 교체 가능하게 추상화
    /// </summary>
    public interface ITransitionEffect
    {
        /// <summary>
        /// 현재 효과가 재생 중인지
        /// </summary>
        bool IsPlaying { get; }
        
        /// <summary>
        /// 화면을 가리는 효과 재생 (맵 언로드 전)
        /// </summary>
        Task PlayOutAsync();
        
        /// <summary>
        /// 화면을 보여주는 효과 재생 (맵 로드 후)
        /// </summary>
        Task PlayInAsync();
        
        /// <summary>
        /// 효과 즉시 중단 및 초기화
        /// </summary>
        void Reset();
    }
}

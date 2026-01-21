using System;
using System.Collections.Generic;

namespace MVP_Core
{
    /// <summary>
    /// ViewModel 기본 클래스
    /// 불변 객체로 설계하여 스레드 안전성 보장
    /// </summary>
    public abstract class ViewModelBase : IEquatable<ViewModelBase>
    {
        /// <summary>
        /// ViewModel 생성 시간
        /// </summary>
        public DateTime CreatedAt { get; }

        protected ViewModelBase()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 동일성 비교 (파생 클래스에서 구현)
        /// </summary>
        public abstract bool Equals(ViewModelBase other);

        public override bool Equals(object obj)
        {
            return obj is ViewModelBase vm && Equals(vm);
        }

        public abstract override int GetHashCode();
    }

    /// <summary>
    /// 슬롯 기반 ViewModel 기본 클래스
    /// 인벤토리, 퀵슬롯, 장비 슬롯 등에 사용
    /// </summary>
    public abstract class SlotViewModelBase : ViewModelBase
    {
        /// <summary>
        /// 슬롯 인덱스
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// 슬롯이 비어있는지 여부
        /// </summary>
        public abstract bool IsEmpty { get; }

        protected SlotViewModelBase(int slotIndex)
        {
            SlotIndex = slotIndex;
        }
    }

    /// <summary>
    /// 리스트 기반 ViewModel 기본 클래스
    /// 아이템 목록, 스탯 목록 등에 사용
    /// </summary>
    /// <typeparam name="TItem">아이템 타입</typeparam>
    public abstract class ListViewModelBase<TItem> : ViewModelBase
    {
        /// <summary>
        /// 아이템 목록 (읽기 전용)
        /// </summary>
        public IReadOnlyList<TItem> Items { get; }

        /// <summary>
        /// 아이템 개수
        /// </summary>
        public int Count => Items?.Count ?? 0;

        /// <summary>
        /// 목록이 비어있는지 여부
        /// </summary>
        public bool IsEmpty => Count == 0;

        protected ListViewModelBase(IEnumerable<TItem> items)
        {
            Items = items != null ? new List<TItem>(items) : new List<TItem>();
        }

        public override bool Equals(ViewModelBase other)
        {
            return other is ListViewModelBase<TItem> listVm && Count == listVm.Count;
        }

        public override int GetHashCode()
        {
            return Count.GetHashCode();
        }
    }
}

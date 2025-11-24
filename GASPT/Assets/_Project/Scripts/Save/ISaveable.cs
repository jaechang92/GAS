namespace GASPT.Save
{
    /// <summary>
    /// 저장 가능한 객체 인터페이스
    /// 이 인터페이스를 구현하는 클래스는 SaveManager를 통해 자동으로 저장/로드할 수 있습니다.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// 저장할 데이터를 반환합니다
        /// </summary>
        /// <returns>직렬화 가능한 데이터 객체</returns>
        object GetSaveData();

        /// <summary>
        /// 저장된 데이터를 불러와 적용합니다
        /// </summary>
        /// <param name="data">직렬화된 데이터 객체</param>
        void LoadFromSaveData(object data);

        /// <summary>
        /// 저장 가능 객체의 고유 ID (선택적)
        /// 여러 인스턴스가 있을 경우 구분용
        /// </summary>
        string SaveID { get; }
    }
}

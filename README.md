# 🎮 Unity 슈퍼마리오 게임 구현 로드맵

> **실무 수준의 게임 아키텍처를 배우는 완전한 가이드**  
> 단순한 게임 만들기를 넘어서, 확장 가능하고 유지보수하기 쉬운 현대적 게임 시스템 구축

[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-blue)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Korean](https://img.shields.io/badge/Language-한국어-red)](README.md)

## 📖 목차

- [전체 강의 구성](#-전체-강의-구성)
- [Part 1: 기본 게임 제작](#-part-1-기본-게임-제작)
- [Part 2: 고급 시스템](#-part-2-고급-시스템)
- [Part 3: 완성도 높이기](#-part-3-완성도-높이기)
- [구현 시 주의사항](#-구현-시-주의사항)
- [최종 결과물](#-최종-결과물)

## 🎯 전체 강의 구성 (총 6-8시간)

### **Part 1: 기본 게임 제작** (2.5-3시간)
### **Part 2: 고급 시스템** (1.5-2시간)  
### **Part 3: 완성도 높이기** (2-3시간)

---

# 📋 Part 1: 기본 게임 제작 (2.5-3시간)

## 🏗️ 1단계: 프로젝트 기반 구축 (30분)

### 1.1 프로젝트 설정 (10분)
- [ ] Unity 2D 프로젝트 생성
- [ ] Git 저장소 초기화
- [ ] .gitignore 설정

### 1.2 폴더 구조 생성 (10분)
```
Assets/
├── Scripts/
│   ├── Core/
│   ├── Gameplay/
│   ├── AI/
│   ├── Audio/
│   └── UI/
├── Art/
│   ├── Sprites/
│   ├── Animations/
│   └── Materials/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Prefabs/
├── Scenes/
└── Resources/
    └── UI/
        └── Panels/
```

### 1.3 Assembly Definition 설정 (10분)
- [ ] Core.asmdef 생성
- [ ] Gameplay.asmdef 생성 (Core 의존)
- [ ] 의존성 관계 설정

---

## 🎭 2단계: 핵심 아키텍처 구현 (45분)

### 2.1 Entity 시스템 (15분)
```csharp
// 구현 순서:
1. Entity.cs (베이스 클래스)
2. ISaveable.cs (인터페이스)
3. Entity 테스트 (빈 GameObject에 붙여서)
```

### 2.2 State Machine 시스템 (20분)
```csharp
// 구현 순서:
1. IState.cs (인터페이스)
2. BaseState.cs (추상 클래스)
3. StateMachine.cs (제네릭 클래스)
4. 간단한 테스트 State 만들어서 동작 확인
```

### 2.3 기본 컴포넌트들 (10분)
```csharp
// 구현 순서:
1. MovementComponent.cs
2. GroundChecker.cs
3. HealthComponent.cs
```

**⚠️ 체크포인트**: State Machine이 정상 동작하는지 테스트

---

## 🏃‍♂️ 3단계: 플레이어 시스템 (40분)

### 3.1 플레이어 상태 정의 (10분)
```csharp
// 구현 순서:
1. PlayerStateType.cs (enum)
2. PlayerController.cs (기본 구조)
3. 컴포넌트 연결 테스트
```

### 3.2 플레이어 상태들 구현 (20분)
```csharp
// 구현 순서:
1. PlayerIdleState.cs
2. PlayerRunningState.cs
3. PlayerJumpingState.cs
4. PlayerFallingState.cs
```

### 3.3 입력 및 물리 연동 (10분)
- [ ] Input System 설정
- [ ] Rigidbody2D 설정
- [ ] Physics2D 레이어 설정

**⚠️ 체크포인트**: 플레이어가 움직이고 점프하는지 테스트

---

## 🗺️ 4단계: 기본 맵 시스템 (35분)

### 4.1 LDtk 맵 임포트 (15분)
- [ ] LDtk 설치 및 기본 맵 제작
- [ ] Unity에 임포트
- [ ] Tilemap 설정

### 4.2 충돌 시스템 (10분)
- [ ] TilemapCollider2D 설정
- [ ] CompositeCollider2D 설정
- [ ] Layer 기반 충돌 설정

### 4.3 카메라 시스템 (10분)
- [ ] CameraFollow 스크립트
- [ ] Cinemachine 또는 커스텀 구현
- [ ] 카메라 바운드 설정

**⚠️ 체크포인트**: 맵에서 플레이어가 자유롭게 움직이는지 테스트

---

## 👾 5단계: 기본 적 시스템 (30분)

### 5.1 적 구조 설계 (10분)
```csharp
// 구현 순서:
1. EnemyController.cs (Entity 상속)
2. EnemyStateType.cs
3. 기본 적 상태들 (Patrol, Idle)
```

### 5.2 기본 AI 구현 (15분)
- [ ] 좌우 이동 패턴
- [ ] 벽 감지
- [ ] 플레이어 충돌 처리

### 5.3 적-플레이어 상호작용 (5분)
- [ ] 밟기 공격
- [ ] 데미지 처리
- [ ] 사망 처리

**⚠️ 체크포인트**: 굼바 스타일 적이 움직이고 플레이어와 상호작용하는지 테스트

---

## 🎵 6단계: 오디오 시스템 (20분)

### 6.1 AudioMixer 설정 (10분)
- [ ] MarioAudioMixer.mixer 생성
- [ ] Master/Music/SFX 그룹 생성
- [ ] Exposed Parameters 설정

### 6.2 AudioManager 구현 (10분)
```csharp
// 구현 순서:
1. AudioSettings.cs (ScriptableObject)
2. AudioManager.cs (기본 PlaySFX, PlayMusic)
3. 기본 사운드 테스트
```

**⚠️ 체크포인트**: 점프 소리, 배경음악이 재생되는지 테스트

---

## 🖥️ 7단계: 기본 UI 시스템 (30분)

### 7.1 UI 프레임워크 구조 (15분)
```csharp
// 구현 순서:
1. PanelState.cs (enum)
2. UIPanel.cs (베이스 클래스)
3. UIManager.cs (Type 기반 관리)
```

### 7.2 기본 패널들 구현 (15분)
- [ ] HUDPanel (체력, 점수 표시)
- [ ] PauseMenuPanel
- [ ] GameOverPanel

**⚠️ 체크포인트**: ESC키로 일시정지 메뉴가 나오는지 테스트

---

## 🎮 8단계: 게임 매니저 & 데이터 (20분)

### 8.1 게임 데이터 시스템 (10분)
```csharp
// 구현 순서:
1. ObservableValue.cs
2. PlayerData.cs
3. GameManager.cs (데이터 관리)
```

### 8.2 UI-Data 연결 (10분)
- [ ] HealthBarUI 구현
- [ ] ScoreUI 구현
- [ ] 자동 업데이트 테스트

**🎉 Part 1 완료**: 기본적인 마리오 게임 완성!

---

# 🚀 Part 2: 고급 시스템 (1.5-2시간)

## 🏢 1단계: 룸 시스템 구현 (30분)

### 1.1 룸 데이터 구조 (10분)
```csharp
// 구현 순서:
1. Room.cs
2. RoomManager.cs
3. 기본 룸 경계 설정
```

### 1.2 스마트 카메라 (20분)
- [ ] CameraController 개선
- [ ] 룸 기반 바운드 설정
- [ ] 부드러운 전환

---

## 🧠 2단계: Decision Tree AI (45분)

### 2.1 Decision Tree 구조 (20분)
```csharp
// 구현 순서:
1. DecisionNode.cs (베이스)
2. DecisionBranch.cs
3. ActionNode.cs
4. 간단한 테스트 트리
```

### 2.2 AI 조건 & 액션 (15분)
- [ ] AIConditions 클래스
- [ ] AIActions 클래스
- [ ] 적 AI에 적용

### 2.3 AI 테스트 & 튜닝 (10분)
- [ ] 다양한 조건 테스트
- [ ] AI 행동 관찰 및 튜닝

---

## ⚡ 3단계: 성능 최적화 (15-30분)

### 3.1 Object Pooling (15분)
- [ ] ObjectPool 제네릭 클래스
- [ ] 적, 이펙트에 적용
- [ ] 메모리 사용량 비교

### 3.2 기타 최적화 (선택사항)
- [ ] Sprite Atlas
- [ ] Audio 압축 설정
- [ ] Build 최적화

**🎉 Part 2 완료**: 대형 맵에서도 부드럽게 돌아가는 게임!

---

# 🎨 Part 3: 완성도 높이기 (2-3시간)

## 👑 1단계: 소닉 보스 시스템 (60분)

### 1.1 보스 기본 구조 (20분)
```csharp
// 구현 순서:
1. BossStateType.cs & BossPhaseType.cs
2. SonicBossController.cs
3. 기본 이동 패턴
```

### 1.2 페이즈 시스템 (20분)
- [ ] 체력 기반 페이즈 전환
- [ ] 페이즈별 다른 행동 패턴
- [ ] 페이즈 전환 연출

### 1.3 보스 UI (20분)
- [ ] BossHealthBarUI
- [ ] 페이즈 표시
- [ ] 보스 등장 연출

---

## 🎬 2단계: 타임라인 시스템 (45분)

### 2.1 타임라인 구조 (20분)
```csharp
// 구현 순서:
1. TimelineEvent.cs (베이스)
2. 구체적 이벤트들 (Move, Attack, Animation, Audio)
3. TimelinePlayer.cs
```

### 2.2 보스전 연출 (15분)
- [ ] 페이즈 전환 타임라인
- [ ] 보스 등장 연출
- [ ] 특수 공격 패턴

### 2.3 타임라인 에디터 (10분, 선택사항)
- [ ] Inspector 개선
- [ ] 미리보기 기능

---

## 💬 3단계: 다이얼로그 시스템 (30분)

### 3.1 다이얼로그 데이터 (10분)
```csharp
// 구현 순서:
1. DialogData.cs
2. DialogCollection.cs (ScriptableObject)
3. CSV 임포트 기능
```

### 3.2 버블 UI (15분)
- [ ] DialogBubble UI 구현
- [ ] 타이프라이터 효과
- [ ] 스마트 포지셔닝

### 3.3 다이얼로그 매니저 (5분)
- [ ] DialogManager.cs
- [ ] Object Pooling 적용

---

## 💾 4단계: 저장 시스템 (45-60분)

### 4.1 저장 데이터 구조 (20분)
```csharp
// 구현 순서:
1. ISaveable.cs 인터페이스
2. PlayerSaveData.cs
3. GameSettingsSaveData.cs
4. LevelObjectSaveData.cs
```

### 4.2 SaveManager 구현 (25분)
- [ ] 바이너리 저장/로드
- [ ] 암호화 시스템
- [ ] 자동 저장 기능

### 4.3 저장 UI & 테스트 (15분)
- [ ] 저장/로드 UI
- [ ] SaveableObject 컴포넌트
- [ ] 세이브 파일 관리

---

## 🌍 5단계: 로컬라이제이션 (30-45분)

### 5.1 로컬라이제이션 구조 (15분)
```csharp
// 구현 순서:
1. Language.cs (enum)
2. LocalizationData.cs
3. LocalizationManager.cs
```

### 5.2 UI 로컬라이제이션 (15분)
- [ ] LocalizedText 컴포넌트
- [ ] 폰트 자동 교체
- [ ] 언어 선택 UI

### 5.3 CSV 데이터 & 테스트 (15분)
- [ ] 로컬라이제이션 CSV 작성
- [ ] 런타임 언어 변경 테스트
- [ ] 모든 UI 텍스트 로컬라이제이션

**🎉 Part 3 완료**: 상용 게임 수준의 완성도!

---

# 🔧 구현 시 주의사항

## ⚠️ 각 단계별 체크포인트

### **절대 다음 단계로 넘어가면 안 되는 조건들:**
1. **2단계 후**: State Machine이 콘솔 에러 없이 동작해야 함
2. **3단계 후**: 플레이어가 자유롭게 움직여야 함
3. **4단계 후**: 맵에서 충돌 감지가 정상 작동해야 함
4. **5단계 후**: 적이 기본 패턴으로 움직여야 함

## 🐛 디버깅 전략

### **단계별 디버깅 도구:**
```csharp
// State Machine 디버깅
[Header("Debug")]
public bool logStateChanges = true;

// AI 디버깅  
void OnDrawGizmosSelected()
{
    // AI 시야, 패트롤 경로 시각화
}

// 데이터 디버깅
[ContextMenu("Print Current Data")]
void PrintDebugData()
{
    Debug.Log($"Health: {playerData.currentHealth.Value}");
}
```

### **자주 발생하는 실수들과 해결책:**

| 문제 | 증상 | 해결책 |
|------|------|--------|
| **State Machine 오류** | 상태 전환이 안 됨 | 조건문과 이벤트 구독 확인 |
| **물리 충돌 문제** | 플레이어가 땅을 뚫고 떨어짐 | Layer Matrix와 Collider 설정 확인 |
| **UI 이벤트 누수** | 씬 전환 시 에러 발생 | OnDestroy에서 이벤트 구독 해제 |
| **오디오 재생 안됨** | 소리가 들리지 않음 | AudioMixer Exposed Parameter 이름 확인 |

## 📚 학습 포인트

### **각 Part에서 강조할 개념:**

#### **Part 1: 기초 아키텍처**
- 🏗️ **아키텍처의 중요성**: 왜 처음부터 구조를 잘 설계해야 하는가
- 🎭 **State Machine 패턴**: 복잡한 상태 관리를 깔끔하게
- 🧩 **컴포넌트 기반 설계**: 재사용 가능한 모듈화
- 📡 **이벤트 기반 프로그래밍**: 느슨한 결합으로 확장성 확보

#### **Part 2: 고급 시스템**
- ⚡ **성능 최적화 기법**: Object Pooling과 메모리 관리
- 🧠 **AI 설계 패턴**: Decision Tree vs State Machine
- 🏢 **대형 프로젝트 관리**: 룸 시스템과 모듈화
- 📊 **프로파일링**: 성능 병목 지점 찾기

#### **Part 3: 상용화 준비**
- 👥 **사용자 경험 (UX)**: 직관적인 UI/UX 설계
- 💾 **데이터 관리**: 안전하고 효율적인 저장 시스템
- 🌍 **국제화 고려사항**: 다국어 지원과 문화적 차이
- 🚀 **상용화 준비**: 실제 출시까지 고려한 완성도

## 🎯 시간 배분 팁

### **시간이 부족할 때 생략 가능한 부분:**
1. **Part 2**: 룸 시스템 (기본 카메라로 대체)
2. **Part 3**: 타임라인 시스템 (간단한 연출로 대체)
3. **Part 3**: 로컬라이제이션 (영어만 지원)

### **절대 생략하면 안 되는 핵심:**
1. **Entity + State Machine** (아키텍처 기반)
2. **UI-Data 연결** (실무 필수 스킬)
3. **오디오 시스템** (게임 완성도)
4. **저장 시스템** (게임의 기본)

---

# 🏆 최종 결과물

각 Part 완료 후 얻는 것:

**Part 1**: 완전히 플레이 가능한 기본 마리오 게임
**Part 2**: 최적화되고 확장 가능한 게임 시스템  
**Part 3**: 상용 게임 수준의 완성도와 기능

**총 학습 시간**: 6-8시간  
**결과물**: 포트폴리오용 고품질 게임 + 실무 활용 가능한 아키텍처

---

## 🔗 관련 리소스

- **GitHub Repository**: [여기에 실제 저장소 링크]
- **동영상 강의**: [여기에 강의 링크]
- **Discord 커뮤니티**: [여기에 Discord 링크]
- **질문/버그 리포트**: [Issues 페이지 링크]

## 🏆 학습 후 얻는 것

### 💻 기술적 역량
- ✅ **현대적 게임 아키텍처** 설계 능력
- ✅ **State Machine 패턴** 완전 이해
- ✅ **컴포넌트 기반 설계** 실무 경험
- ✅ **성능 최적화** 기법 습득
- ✅ **UI/UX 시스템** 구축 능력

### 🎮 완성된 게임
- ✅ **플레이 가능한 마리오 게임**
- ✅ **3개 국어 지원** (한/영/일)
- ✅ **저장/불러오기** 시스템
- ✅ **보스전** 및 **AI 시스템**
- ✅ **상용 게임 수준**의 완성도

### 📱 실무 적용
- ✅ **어떤 장르 게임**에도 적용 가능한 기반 아키텍처
- ✅ **팀 프로젝트**에서 바로 사용 가능한 모듈화 구조
- ✅ **확장성**을 고려한 설계 경험
- ✅ **유지보수**가 쉬운 코드 작성법

---

## 📝 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

## 🤝 기여하기

버그 리포트, 기능 제안, 풀 리퀘스트를 환영합니다!

1. 이 저장소를 Fork 해주세요
2. 새로운 브랜치를 생성해주세요 (`git checkout -b feature/amazing-feature`)
3. 변경사항을 커밋해주세요 (`git commit -m 'Add amazing feature'`)
4. 브랜치에 Push 해주세요 (`git push origin feature/amazing-feature`)
5. Pull Request를 열어주세요

## 📧 연락처

궁금한 점이 있으시면 언제든 연락해주세요:
- **Email**: [여기에 이메일]
- **Discord**: [여기에 Discord]
- **GitHub Issues**: [Issues 페이지]

---

⭐ **이 프로젝트가 도움이 되셨다면 Star를 눌러주세요!**

# iOS 빌드 + TestFlight 가이드 v1

> Little Hero by the Lake — App Store 1차 출시 (Phase G4·G5)
>
> 작성: 2026-05-01 (G4-1·G4-2 점검·자동 보강 직후)

---

## 0. 자동 보강 완료 항목 (G4-2 사이클)

다음은 Unity Editor 측에서 이미 적용됨:

| 항목 | 값 |
|---|---|
| ProductName | `LittleHero` |
| CompanyName | `Bandibul` |
| BundleID (iOS) | `com.bandibul.littlehero` |
| BuildNumber (iOS) | `1` |
| Build Scenes | Attic + Chapter + InfiniteTower (3개) |
| Default Orientation | Portrait |
| Allowed Orientations | Portrait only (4방향 잠금) |
| ScriptingBackend (iOS) | IL2CPP |
| Architecture (iOS) | ARM64 |
| TargetDevice | iPhoneAndiPad |
| TargetOSVersion | 15.0 |

## 1. 수동 변경 필요 (사용자가 Unity Editor 에서)

### 1.1 Api Compatibility Level (CLAUDE.md §1 정합)

`Edit` > `Project Settings` > `Player` > `iOS` 탭 > `Other Settings` > `Configuration` > `Api Compatibility Level`
- 현재: `.NET Standard 2.0`
- 변경: **`.NET Standard 2.1`** (Unity 6 의 enum 라벨은 `NET_Standard` 또는 `.NET Standard 2.1`)

### 1.2 앱 아이콘 (App Store 거부 위험 1순위)

`Edit` > `Project Settings` > `Player` > `iOS` 탭 > `Icon`
- 19개 슬롯에 PNG 등록 필요 (최소 `App Store 1024×1024`)
- 후보 크기: `1024×1024 / 180×180 / 167×167 / 152×152 / 120×120 / 87×87 / 80×80 / 76×76 / 60×60 / 58×58 / 40×40 / 29×29 / 20×20`
- Phase G2 P0~P5 의 plush 톤 reference 활용 권장 (Char_BraveGirl·Char_DefaultBoy·Char_QuietFriend 의 아이덴티티 핵심 원소: 호숫가·다락방·동화책·녹색 후드)
- **임시 placeholder** 도 가능 (TestFlight 베타용). App Store 본 심사 전 정식 아트로 교체.

### 1.3 Apple Developer Team ID (사용자 인증 후)

`Edit` > `Project Settings` > `Player` > `iOS` 탭 > `Identification` > `Signing Team ID`
- 현재: 빈 값
- Apple Developer Program 등록 후 받는 10자 영숫자 코드 입력
- 또는 **Xcode 빌드 단계에서 입력** 가능 (Unity 측 비워둬도 됨)

## 2. Apple Developer Program 등록 ($99/년)

1. https://developer.apple.com/programs/ 접속
2. Apple ID 로 로그인 → Enroll
3. 개인 (Individual) 또는 법인 (Organization) 선택
   - **개인**: 1~2주 인증, 본인 이름이 App Store 에 노출
   - **법인 (Bandibul 명의)**: D-U-N-S 번호 필요 (무료, 한국 dnb.co.kr), 등록사업자 인증 필요
4. 결제 ($99 또는 약 14만 원)
5. 인증 완료 후 Team ID 발급 (10자 영숫자)

## 3. Unity → Xcode 프로젝트 출력

1. `File` > `Build Settings` 열기
2. `Platform` = iOS 확인 (이미 설정됨)
3. `Build` 또는 `Build And Run` 클릭
4. 출력 폴더 지정 (예: `~/git/little-hero-by-the-lake/build/ios`)
5. Unity 가 IL2CPP 컴파일 + Xcode 프로젝트 생성 (10~30분 소요)
6. 출력 폴더의 `Unity-iPhone.xcodeproj` 자동 열림

## 4. Xcode 단계

### 4.1 Signing & Capabilities

1. 좌측 Project Navigator 에서 `Unity-iPhone` 프로젝트 클릭
2. `Signing & Capabilities` 탭
3. `Automatically manage signing` 체크
4. `Team` 드롭다운에서 본인 Apple Developer Team 선택
5. `Bundle Identifier` 가 `com.bandibul.littlehero` 인지 확인
6. `Provisioning Profile` 자동 생성 확인

### 4.2 Capabilities 추가

`+ Capability` 클릭해서 다음 추가:
- **In-App Purchase** (E1 IAPService 6 productId 사용)
- **Push Notifications** (선택, 시즌 패스/일일 보스 알림용 — 출시 후 추가 가능)

### 4.3 Info.plist 점검

`Info.plist` 파일에서 다음 키 확인:
- `NSCameraUsageDescription` / `NSMicrophoneUsageDescription` / `NSLocationWhenInUseUsageDescription` — **사용 안 함** → key 자체를 삭제 (있으면 Apple 심사 시 사용 이유 묻습니다)
- `ITSAppUsesNonExemptEncryption` = `NO` (네트워크 통신은 IAP 의 표준 SSL 만 — 비표준 암호화 없음)
- `LSRequiresIPhoneOS` = `YES`
- `UISupportedInterfaceOrientations` = `UIInterfaceOrientationPortrait` only

### 4.4 Build → Archive

1. Xcode 상단 device 선택 → `Any iOS Device (arm64)`
2. `Product` > `Archive`
3. Organizer 창 자동 열림

### 4.5 Distribute App

1. Organizer 의 archive 선택 → `Distribute App` 클릭
2. `App Store Connect` 선택 → Next
3. `Upload` 선택 → Next
4. Distribution Options 모두 기본값 → Next
5. Apple ID 로그인
6. 업로드 완료 (5~15분)

## 5. App Store Connect 단계

### 5.1 앱 등록

1. https://appstoreconnect.apple.com 접속
2. `My Apps` > `+` > `New App`
3. 입력:
   - Platforms: `iOS`
   - Name: `호숫가 작은 영웅` (한국어 1차) / `Little Hero by the Lake` (영문)
   - Primary Language: `Korean`
   - Bundle ID: `com.bandibul.littlehero` (드롭다운에서 선택)
   - SKU: `littlehero-ios-001` (임의 unique 식별자)
   - User Access: `Full Access`

### 5.2 IAP 6 productId 등록

`In-App Purchases` > `Manage` > `+` 로 6개 등록:

| productId | 종류 | 가격 (KRW) | 비고 |
|---|---|---|---|
| `STARLIGHT_100` | Consumable | 1,200 | 100 별빛 |
| `STARLIGHT_500` | Consumable | 5,500 | 500+50 별빛 |
| `STARLIGHT_1100` | Consumable | 11,000 | 1,100+120 별빛 |
| `STARLIGHT_3500` | Consumable | 33,000 | 3,500+400 별빛 |
| `STARLIGHT_12000` | Consumable | 110,000 | 12,000+1,500 별빛 |
| `SEASON_PASS` | Non-Consumable | 11,000 | 시즌당 1회 (시즌 종료 시 RotateSeason) |

각 productId 의 한국어/영문 displayName + description 입력. 4+ 등급 정합 위해 **확률성 보상 NOT 명시** (별빛 패키지는 결정론적 수량). 주의: 시즌 패스는 비소비형 — Apple 정책상 동일 productId 재구매 시 무료 (Restore Purchases 처리).

### 5.3 4+ 등급 인증

`App Information` > `Age Rating`
- 모든 카테고리 `None` 선택 (폭력·도박·성·약물 등 모두 없음)
- 결과: `4+` 자동 산출
- **별빛 상점이 가챠로 오해받지 않도록**: description 에 "결정적 수량 구매 — 임의성 없음" 명시

### 5.4 메타데이터 입력

`App Store` > `App Information` + `Version 1.0` 작성:
- Promotional Text (170자): "다락방에서 발견한 아빠의 동화책 — 호숫가 마을 어린이의 따뜻한 모험"
- Description (4000자): 게임 소개·핵심 메커닉(자동 슈팅 + 챕터 진행 + 4 화폐)·비폭력 톤 강조·플레이어 연령대(가족·캐주얼)
- Keywords (100자): `호숫가,동화책,로그라이트,귀여운,4+,오토슈팅,모험,어린이,캐주얼,한국어`
- Screenshots: iPhone 6.7" + 6.5" + 5.5" 각 3~10장 (다락방·신기한 지도·챕터 플레이·인벤토리·캐릭터 정보)
- App Icon: 1024×1024 (5.2 와 별도 — App Store 노출용)
- Support URL: 사용자 보유 (아직 없으면 `https://github.com/MoonjaeChoi/little-hero-by-the-lake` 임시)
- Privacy Policy URL: 필수 (별빛 IAP 사용으로 결제 정보 처리 — Apple 자동 분석. 별도 페이지 작성 필요)

### 5.5 Privacy 설정 필수

`App Privacy` > `Get Started`
- 데이터 수집: `Yes` (IAP 결제 정보)
- 카테고리: `Purchases` > `Purchase History` (Linked to User · Used for App Functionality)

## 6. TestFlight (베타 테스트)

1. Xcode 에서 업로드한 빌드가 App Store Connect 에 자동 등록 (10~30분 후 `Processing` → `Ready to Submit`)
2. `TestFlight` 탭 > 빌드 선택
3. `Internal Testing` 그룹 추가 (최대 100명, 본인 + 지인)
4. 또는 `External Testing` 그룹 (최대 10,000명, Apple 베타 심사 1~2일)
5. 테스터에게 TestFlight 앱 설치 링크 발송
6. 1~2주 베타 → 피드백 수집 → 버그 fix → BuildNumber 증분 → 재업로드 반복

## 7. App Store 심사 제출

1. 베타 안정 확인 후 `App Store` 탭 > `Submit for Review`
2. Apple 심사 1~7일 (4+ 등급 + IAP 사용으로 평균 3일)
3. 거부 사유 흔한 케이스:
   - 아이콘 placeholder 검출 → 정식 아트로 교체
   - 메타데이터 오류 → description·screenshots 정정
   - IAP 의 RestorePurchases 미구현 → 시즌 패스 복구 흐름 확인
   - Privacy Policy URL dead link → URL 정상 응답 확인
4. 승인 → 출시 일정 (수동 출시 또는 자동 즉시 출시)

## 8. 출시 후 후속 (Phase G6)

| 항목 | 시점 | 내용 |
|---|---|---|
| 시즌 1 시작 | 출시 직후 | E4 SeasonPassService.RotateSeason 호출 |
| 데이터 수집 | 출시 1주 | DAU·ARPU·1일 retention·이탈 챕터 분포 |
| 첫 패치 | 출시 2~4주 | 베타 측정 기반 BalanceConfig 미세 조정 (시뮬 §13 위험 보정 후속) |
| 시즌 2 | 3개월차 | 코스메틱 추가, RotateSeason |

## 9. 알려진 후속 작업 (출시 전 정리 권장)

| 우선순위 | 작업 | 위치 |
|---|---|---|
| 🟡 | AtticController 메뉴 패널 토글 (도감/지도 동시 활성화 방지) | `Hub/AtticController.cs` |
| 🟡 | SaveData.firstClearedChapters int → long 승격 (CH 33~35 잠금 해제) | `Core/SaveData.cs` |
| 🟢 | EnemyPool_Default 에 V2 41 적 등록 (도감 정합) | `Data/Enemies/EnemyPool_Default.asset` |
| 🟢 | WondrousMapPanel 배경 alpha 1.0 (raycast 누설 방지) | UI prefab |
| 🟢 | AudioManager `time on non-clip` 경고 정리 | `Core/AudioManager.cs` |
| 🔵 | F1~F3 placeholder 적 디자인 패스 | `Data/Enemies/Enemy_*_*.asset` |
| 🔵 | F4·F5 실 보스 씬 분리 (현재 InstantComplete 시뮬) | `Scenes/BossEncounter.unity` 신설 |

## 10. 출처·정합

- `CLAUDE.md` §1 (.NET Standard 2.1) / §2 (4+ 등급 + 한 손 플레이) / §11 마일스톤
- `docs/little_hero/monetization_v1.md` §7.4 (IAP 6 productId)
- Apple App Store Review Guidelines (https://developer.apple.com/app-store/review/guidelines/)

---

> **작성 후 검수 완료 시점**: 2026-05-01
> **다음 갱신**: 첫 TestFlight 업로드 결과 반영

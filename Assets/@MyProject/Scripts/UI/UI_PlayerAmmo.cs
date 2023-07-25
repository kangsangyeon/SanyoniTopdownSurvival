using System;
using FishNet;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace MyProject
{
    [ExecuteAlways]
    public class UI_PlayerAmmo : ImmediateModeShapeDrawer
    {
        [SerializeField] private Camera m_Cam;
        [SerializeField] private Transform m_CenterTransform;
        [SerializeField] private Player m_Player;

        private IGunWeapon m_GunWeapon;

        [Header("Sidebar Style")] [SerializeField] [Range(0, 0.2f)]
        private float m_AmmoBarThickness;

        [SerializeField] [Range(0, 0.05f)] private float m_AmmoBarOutlineThickness = 0.1f;

        [SerializeField] [Range(0f, ShapesMath.TAU / 2)]
        private float m_AmmoBarAngularSpanRad;

        [SerializeField] private float m_AmmoBarRotationOffsetRad;
        [SerializeField] private float m_BarRadius;

        [Header("Style")] [SerializeField] private Color m_Color = Color.cyan;
        [SerializeField] [Range(0, 1f)] private float m_BulletThicknessScale = 1f;

        [Header("Animation")] [SerializeField] private float m_BulletDisappearTime = 1f;

        [SerializeField] [Range(0, ShapesMath.TAU)]
        private float m_BulletEjectAngSpeed = 0.5f;

        [SerializeField] [Range(0, ShapesMath.TAU)]
        private float m_EjectRotSpeedVariance = 1f;

        [SerializeField] private AnimationCurve m_BulletEjectX = AnimationCurve.Constant(0, 1, 0);
        [SerializeField] private AnimationCurve m_BulletEjectY = AnimationCurve.Constant(0, 1, 0);
        [SerializeField] [Range(0, 0.5f)] private float m_BulletEjectScale = 0.5f;

        private int m_LastFiredBulletIndex = 15;
        private float[] m_BulletFireTimes;
        private bool m_ShouldDraw = true;
        private Action m_GunWeapon_OnCurrentMagazineCountChangedAction;

        public void DrawBar()
        {
            float _barThickness = m_AmmoBarThickness;
            float _angRadMin = m_AmmoBarRotationOffsetRad + -m_AmmoBarAngularSpanRad / 2;
            float _angRadMax = m_AmmoBarRotationOffsetRad + m_AmmoBarAngularSpanRad / 2;

            // 총알들을 그립니다.
            Draw.LineEndCaps = LineEndCap.Round;
            float _innerRadius = m_BarRadius - _barThickness / 2;
            float _bulletThickness =
                (_innerRadius * m_AmmoBarAngularSpanRad / m_GunWeapon.maxMagazineCount)
                * m_BulletThicknessScale;

            for (int _i = 0; _i < m_GunWeapon.maxMagazineCount; _i++)
            {
                float _t = _i / (m_GunWeapon.maxMagazineCount - 1f);
                float _angRad = Mathf.Lerp(_angRadMin, _angRadMax, _t);
                Vector2 _dir = ShapesMath.AngToDir(_angRad);
                Vector2 _origin = _dir * m_BarRadius;
                Vector2 _offset = _dir * (_barThickness / 2f - m_AmmoBarOutlineThickness * 1.5f);

                // 발사한 총알이라면, 애니메이션된 위치와 alpha를 얻습니다.
                float _alpha = 1;
                bool _hasBeenFired = _i >= m_LastFiredBulletIndex;
                if (_hasBeenFired && Application.isPlaying)
                {
                    float _timePassed = Time.time - m_BulletFireTimes[_i];
                    float _tFade = Mathf.Clamp01(_timePassed / m_BulletDisappearTime);
                    _alpha = 1f - _tFade;
                    _origin = GetBulletEjectPos(_origin, _tFade);
                    float _angle =
                        _timePassed * (m_BulletEjectAngSpeed + Mathf.Cos(_i * 92372.8f) * m_EjectRotSpeedVariance);
                    _offset = ShapesMath.Rotate(_offset, _angle);
                }

                Vector2 _a = _origin + _offset;
                Vector2 _b = _origin - _offset;
                Draw.Line(_a, _b, _bulletThickness, new Color(m_Color.r, m_Color.g, m_Color.b, _alpha));
            }

            // 총알들을 감싸는 원호를 그립니다.
            DrawRoundedArcOutline(
                Vector2.zero, m_BarRadius,
                _barThickness, m_AmmoBarOutlineThickness,
                _angRadMin, _angRadMax,
                m_Color);
        }

        Vector2 GetBulletEjectPos(Vector2 _origin, float _t)
        {
            Vector2 _ejectAnimPos = new Vector2(m_BulletEjectX.Evaluate(_t), m_BulletEjectY.Evaluate(_t));
            return _origin + _ejectAnimPos * m_BulletEjectScale;
        }

        public static void DrawRoundedArcOutline(
            Vector2 _origin, float _radius,
            float _thickness, float _outlineThickness,
            float _angStart, float _angEnd,
            Color _color)
        {
            DiscColors _colors = new DiscColors()
                { innerStart = _color, innerEnd = _color, outerStart = _color, outerEnd = _color };

            // inner / outer
            float _innerRadius = _radius - _thickness / 2;
            float _outerRadius = _radius + _thickness / 2;
            const float AA_MARGIN = 0.01f;
            Draw.Arc(_origin, _innerRadius, _outlineThickness,
                _angStart - AA_MARGIN, _angEnd + AA_MARGIN,
                _colors);
            Draw.Arc(_origin, _outerRadius, _outlineThickness,
                _angStart - AA_MARGIN, _angEnd + AA_MARGIN,
                _colors);

            // rounded caps
            Vector2 _originBottom = _origin + ShapesMath.AngToDir(_angStart) * _radius;
            Vector2 _originTop = _origin + ShapesMath.AngToDir(_angEnd) * _radius;
            Draw.Arc(_originBottom, _thickness / 2, _outlineThickness,
                _angStart, _angStart - ShapesMath.TAU / 2,
                _colors);
            Draw.Arc(_originTop, _thickness / 2, _outlineThickness, _angEnd, _angEnd + ShapesMath.TAU / 2, _colors);
        }

        public override void DrawShapes(Camera _cam)
        {
            // if (_cam != this.m_Cam) // only draw in the player camera
            //     return;

            if (m_ShouldDraw == false)
                return;

            if (m_GunWeapon == null)
                return;

            using (Draw.Command(_cam))
            {
                Draw.ZTest = CompareFunction.Always; // to make sure it draws on top of everything like a HUD
                Draw.Matrix = m_CenterTransform.localToWorldMatrix; // draw it in the space of crosshairTransform
                Draw.BlendMode = ShapesBlendMode.Transparent;
                Draw.LineGeometry = LineGeometry.Flat2D;
                DrawBar();
            }
        }

        private void Start()
        {
            if (m_Player.IsOwner == false)
            {
                this.enabled = false;
                return;
            }

            if (m_Player.weapon is IGunWeapon _gunWeapon)
                InitializeGunWeaponEvents(_gunWeapon);

            m_Player.onWeaponChanged_OnServer += (_prevWeapon, _newWeapon) =>
            {
                if (_prevWeapon != null)
                {
                    if (_prevWeapon is IGunWeapon _prevGunWeapon)
                        UninitializeGunWeaponEvents(_prevGunWeapon);
                }

                if (_newWeapon is IGunWeapon _gunWeapon)
                    InitializeGunWeaponEvents(_gunWeapon);
            };

            m_Player.health.onHealthIsZero_OnClient += () => { m_ShouldDraw = false; };

            m_Player.health.onHealthChanged_OnClient += _i =>
            {
                if (m_Player.health.health > 0)
                    m_ShouldDraw = true;
            };
        }

        private void InitializeGunWeaponEvents(IGunWeapon _gunWeapon)
        {
            m_GunWeapon = _gunWeapon;

            m_BulletFireTimes = new float[m_GunWeapon.maxMagazineCount + 1];
            // m_GunWeapon.onMaxMagazineCountChanged += () =>
            //     m_BulletFireTimes = new float[m_GunWeapon.maxMagazineCount + 1];

            m_LastFiredBulletIndex = m_GunWeapon.currentMagazineCount;

            m_GunWeapon_OnCurrentMagazineCountChangedAction = () =>
            {
                m_LastFiredBulletIndex = m_GunWeapon.currentMagazineCount;
                m_BulletFireTimes[m_LastFiredBulletIndex] = Time.time;
            };
            m_GunWeapon.onCurrentMagazineCountChanged += m_GunWeapon_OnCurrentMagazineCountChangedAction;
        }

        private void UninitializeGunWeaponEvents(IGunWeapon _gunWeapon)
        {
            _gunWeapon.onCurrentMagazineCountChanged -= m_GunWeapon_OnCurrentMagazineCountChangedAction;
            m_GunWeapon_OnCurrentMagazineCountChangedAction = null;

            m_GunWeapon = null;
        }
    }
}
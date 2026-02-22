using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Balatro101.Game.Core;
using Balatro101.Game.Engine;

namespace Balatro101.Game.UI;

public static class ScoreAnimator
{
    public enum ScorePhase
    {
        Idle,
        WindUp,
        BaseScore,
        EvaluatingCards,
        EvaluatingJokers,
        Resolution
    }

    public static ScorePhase CurrentPhase { get; private set; } = ScorePhase.Idle;

    private static GameManager? _game;
    private static HandResult? _handResult;
    private static ScoreState? _finalState;
    private static int _finalTotalScore;

    // Display values
    public static int DisplayChips { get; private set; }
    public static int DisplayMult { get; private set; }

    // UI Juice properties
    public static float BoxesScale => _boxesScale;
    private static float _boxesScale = 1.0f;
    private static float _boxesTargetScale = 1.0f;
    private static float _boxesVelocityScale = 0f;

    // Timers & Indices
    private static float _phaseTimer;
    private static int _evalIndex;
    private static float _subTimer;

    // Internal trackers to replay the math step-by-step
    private static int _currentMathChips;
    private static int _currentMathMult;

    private static readonly List<FloatingText> _flyingTexts = new();

    public static bool IsAnimating => CurrentPhase != ScorePhase.Idle;

    public static void StartSequence(GameManager game, HandResult result, ScoreState finalState, int finalTotalScore)
    {
        _game = game;
        _handResult = result;
        _finalState = finalState;
        _finalTotalScore = finalTotalScore;

        DisplayChips = 0;
        DisplayMult = 0;

        CurrentPhase = ScorePhase.WindUp;
        _phaseTimer = 0.5f;

        _currentMathChips = result.BaseChips;
        _currentMathMult = result.BaseMultiplier;

        _flyingTexts.Clear();

        // Faz as cartas que nao vao pontuar descerem para fora da tela
        foreach (var c in game.PlayerHand)
        {
            if (!c.IsSelected)
            {
                c.TargetY = GameConfig.WindowHeight + 200f;
            }
        }

        // Centraliza as cartas que vao pontuar
        int totalSelected = result.ScoringCards.Count;
        float startX = (GameConfig.WindowWidth - (totalSelected * RendererUtils.CardWidth)) / 2f + (RendererUtils.CardWidth / 2f);
        for (int i = 0; i < totalSelected; i++)
        {
            var c = result.ScoringCards[i];
            c.TargetX = startX + i * RendererUtils.CardWidth;
            c.TargetY = GameConfig.WindowHeight / 2f - 20f;
            c.TargetScale = 1.0f;
        }
    }

    public static void UpdateAndDraw(float dt)
    {
        if (CurrentPhase == ScorePhase.Idle || _game == null || _handResult == null || _finalState == null) return;

        // Draw background gameplay
        GameUI.DrawGameplay(_game);

        // Physics for the HUD boxes
        TweenEngine.ApplySpring(ref _boxesScale, ref _boxesVelocityScale, _boxesTargetScale, dt);

        for (int i = _flyingTexts.Count - 1; i >= 0; i--)
        {
            _flyingTexts[i].Update(dt);
            _flyingTexts[i].Draw();
            if (_flyingTexts[i].HitTarget && _flyingTexts[i].Age >= _flyingTexts[i].Lifetime)
            {
                // When hits the HUD, bump the HUD scale
                BumpHUD();
                _game.ShakeTimer = 0.2f; // Mini shake
                _game.ShakeMagnitude = 5f;
                _flyingTexts.RemoveAt(i);
            }
        }

        switch (CurrentPhase)
        {
            case ScorePhase.WindUp:
                _phaseTimer -= dt;

                // Show name of the hand popping
                if (_phaseTimer < 0.3f)
                {
                    int textW = Raylib.MeasureText(_handResult.Type.ToString(), 50);
                    Raylib.DrawText(_handResult.Type.ToString(), (GameConfig.WindowWidth - textW) / 2, GameConfig.WindowHeight / 2 - 200, 50, Color.Gold);
                }

                if (_phaseTimer <= 0)
                {
                    CurrentPhase = ScorePhase.BaseScore;
                    _phaseTimer = 0.6f;
                    DisplayChips = _handResult.BaseChips;
                    DisplayMult = _handResult.BaseMultiplier;
                    BumpHUD();
                    _game.ShakeTimer = 0.3f;
                    _game.ShakeMagnitude = 10f;
                }
                break;

            case ScorePhase.BaseScore:
                _phaseTimer -= dt;
                if (_phaseTimer <= 0)
                {
                    CurrentPhase = ScorePhase.EvaluatingCards;
                    _evalIndex = 0;
                    _subTimer = 0.5f;
                }
                break;

            case ScorePhase.EvaluatingCards:
                {
                    _subTimer -= dt;

                    if (_subTimer <= 0 && _evalIndex < _handResult.ScoringCards.Count)
                    {
                        var card = _handResult.ScoringCards[_evalIndex];
                        card.BumpScale(1.3f);

                        int val = card.GetValue() + card.BonusChips;
                        if (val > 0)
                        {
                            var ft = new FloatingText($"+{val}", Color.Blue, new Vector2(card.X, card.Y), new Vector2(72f, 272f), 0.4f);
                            _flyingTexts.Add(ft);
                            _currentMathChips += val;
                            DisplayChips = _currentMathChips; // Sync immediately for visual, juice happens on impact
                        }
                        if (card.BonusMult > 0)
                        {
                            var ft = new FloatingText($"+{card.BonusMult}", Color.Red, new Vector2(card.X, card.Y), new Vector2(197f, 272f), 0.4f);
                            _flyingTexts.Add(ft);
                            _currentMathMult += card.BonusMult;
                            DisplayMult = _currentMathMult;
                        }

                        // Card local Jokers (pseudo execution to keep math consistent)
                        foreach (var joker in _game.ScoringEngine.ActiveJokers)
                        {
                            var mockState = new ScoreState { Chips = _currentMathChips, Multiplier = _currentMathMult };
                            joker.OnCardScored(card, mockState);

                            int diffChips = mockState.Chips - _currentMathChips;
                            int diffMult = mockState.Multiplier - _currentMathMult;

                            if (diffChips > 0 || diffMult > 0)
                            {
                                // Could spawn joker specific texts here from card
                                _currentMathChips = mockState.Chips;
                                _currentMathMult = mockState.Multiplier;
                                DisplayChips = _currentMathChips;
                                DisplayMult = _currentMathMult;
                            }
                        }

                        _evalIndex++;
                        _subTimer = 0.4f;
                    }
                    else if (_evalIndex >= _handResult.ScoringCards.Count && _flyingTexts.Count == 0)
                    {
                        CurrentPhase = ScorePhase.EvaluatingJokers;
                        _evalIndex = 0;
                        _subTimer = 0.5f;
                    }
                    break;
                }

            case ScorePhase.EvaluatingJokers:
                {
                    _subTimer -= dt;

                    if (_subTimer <= 0 && _evalIndex < _game.ScoringEngine.ActiveJokers.Count)
                    {
                        var joker = _game.ScoringEngine.ActiveJokers[_evalIndex];

                        var mockState = new ScoreState { Chips = _currentMathChips, Multiplier = _currentMathMult };
                        joker.OnHandEvaluated(_handResult, mockState);

                        int diffChips = mockState.Chips - _currentMathChips;
                        int diffMult = mockState.Multiplier - _currentMathMult;
                        int multMultiplier = mockState.Multiplier > 0 && _currentMathMult > 0 && mockState.Multiplier % _currentMathMult == 0 ? mockState.Multiplier / _currentMathMult : 0;

                        if (diffChips > 0 || diffMult > 0)
                        {
                            var jRect = GameUI.GetJokerRect(_evalIndex);
                            float jx = jRect.X + jRect.Width / 2f;
                            float jy = jRect.Y + jRect.Height / 2f;

                            joker.Wobble(30f);
                            joker.BumpScale(1.4f);

                            if (diffChips > 0)
                            {
                                _flyingTexts.Add(new FloatingText($"+{diffChips}", Color.Blue, new Vector2(jx, jy), new Vector2(72f, 272f), 0.4f));
                            }
                            else if (multMultiplier > 1)
                            {
                                _flyingTexts.Add(new FloatingText($"X{multMultiplier}", Color.Red, new Vector2(jx, jy), new Vector2(197f, 272f), 0.4f));
                            }
                            else if (diffMult > 0)
                            {
                                _flyingTexts.Add(new FloatingText($"+{diffMult}", Color.Red, new Vector2(jx, jy), new Vector2(197f, 272f), 0.4f));
                            }

                            _currentMathChips = mockState.Chips;
                            _currentMathMult = mockState.Multiplier;
                            DisplayChips = _currentMathChips;
                            DisplayMult = _currentMathMult;

                            _game.ShakeTimer = 0.4f;
                            _game.ShakeMagnitude = 15f;
                        }

                        _evalIndex++;
                        _subTimer = 0.4f;
                    }
                    else if (_evalIndex >= _game.ScoringEngine.ActiveJokers.Count && _flyingTexts.Count == 0)
                    {
                        CurrentPhase = ScorePhase.Resolution;
                        _phaseTimer = 1.0f;
                        DisplayChips = _finalState.Chips;
                        DisplayMult = _finalState.Multiplier;
                        BumpHUD();
                        _game.ShakeTimer = 0.5f;
                        _game.ShakeMagnitude = 20f;
                    }
                    break;
                }

            case ScorePhase.Resolution:
                _phaseTimer -= dt;

                string finalScore = $"{_finalTotalScore}";
                int fw = Raylib.MeasureText(finalScore, 40);

                // Draw it bursting out of the Left Panel boxes
                float bounceY = 200f - MathF.Sin(_phaseTimer * MathF.PI) * 20f;
                Raylib.DrawText(finalScore, 135 - (fw / 2), (int)bounceY, 40, Color.Green);

                if (_phaseTimer <= 0)
                {
                    // Clean up
                    foreach (var c in _handResult.ScoringCards)
                    {
                        _game.PlayerHand.Remove(c);
                        c.TargetX = GameConfig.WindowWidth + 200; // Sweep to discard
                    }
                    _game.CurrentScore += _finalTotalScore;
                    _game.FinishScoreAnimation();
                    CurrentPhase = ScorePhase.Idle;
                }
                break;
        }

        // Keep updating physics for the cards on screen
        foreach (var card in _game.PlayerHand)
        {
            TweenEngine.UpdatePhysics(card, dt);
            RendererUtils.DrawCard(card);
        }
    }

    private static void BumpHUD()
    {
        _boxesTargetScale = 1.0f;
        _boxesVelocityScale = 10f; // Give velocity instead of hard scale to bounce
    }
}

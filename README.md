# Balatro 101 Prototype

Bem-vindo ao **Balatro 101 Prototype**, um clone do popular jogo de construção de baralhos roguelike "Balatro". Este projeto é desenvolvido em C# utilizando a biblioteca Raylib-cs. Após extensas atualizações e refatorações, o projeto evoluiu de uma renderização puramente procedimental para utilizar **Sprites 2D Reais**, implementar **Controle de Gamepad (Suporte a Controle)**, e um **Sistema de Animação Avançado Passo-a-Passo** (Score Animator).

Este `README.md` documenta em detalhes o estado atual do projeto: seu objetivo, funcionamento, regras e a organização de sua arquitetura robusta. 

---

## 🎯 Objetivo do Jogo

O objetivo do jogador em **Balatro 101** é acumular pontos suficientes para atingir ou superar uma **Pontuação Alvo** em cada *Blind* (Desafio). O jogador possui um número limitado de "Mãos" para jogar e de "Descartes" para alterar a sorte da mão.

A jornada (Run) escala infinitamente em *Antes*. Cada Ante possui 3 *Blinds*: **Small Blind**, **Big Blind**, e um **Boss Blind**.
*Boss Blinds* impõem punições severas conhecidas como *Debuffs* (ex: "The Window" corta descartes pela metade, "The Pillar" retira uma mão jogável, "The Hook" descarta aleatoriamente cartas após jogar).

Ao vencer Blinds, você adquire dinheiro (`$`) com bônus por sobras de mãos e parte para a Loja (`Shop`) para melhorar a sinergia comprando Tarôs, Planetas e os exóticos Jokers. 

---

## ⚙️ Como o Jogo Funciona e Arquitetura Visual

O projeto agora é enriquecido visualmente graças a três novos pilares principais sob o pacote `Game.UI`:

1. **`AssetManager`**: Rompendo a restrição de primitivas desenhadas, o jogo recorta as sprites a partir da pasta `/Assets`. Carrega cartas, versos de cartas, Jokers desenhados, e Consumíveis (Tarô e Planetas).
2. **`ScoreAnimator` & `ParticleSystem`**: O coração do *Juice* do jogo. Ao avaliar uma mão, o jogo transiciona pela Phase FSM de pontuação (WindUp -> BaseScore -> EvaluatingCards -> EvaluatingJokers -> Resolution). Cada carta e Joker é calculado sequencialmente, emitindo `FloatingTexts` arcados e efeitos explosivos de Partículas (+ Tremor de tela via `ShakeMagnitude`).
3. **`InputManager` e `VirtualCursor`**: Permite transição *Seamless* entre Mouse e **Gamepads** Genéricos (incluindo controles de Xbox/PlayStation). Direcionais e analógicos movem uma engrenagem de navegação `UINode` ou manipulam o Joystick para jogar.

O fluxo FSM (`GameState`) foi estendido para: `MainMenu -> Shop -> Shuffling -> FaseGameplay -> AnimacaoPontuacao -> ShopTransition -> (Loop)`.

---

## 📜 Regras Atuais Implementadas

### Limites e Multiplicadores Base

- **Baralho**: 52 cartas carregadas via Assets Reais.
- **Mão do Jogador**: Mantém automaticamente completadas até **8 cartas**. É sacada com animações *Spring* após o estado `Shuffling`.
- **Máximo de Mãos Jogáveis**: Padrão **4**.
- **Máximo de Descartes**: Padrão **3**.
- **Máximo de Cartas Selecionadas**: Até **5 cartas** por ação.
- **Pontuação Alvo**: Escala atrelada ao Ante e Tipo de Blind (`300 * CurrentAnte * BlindMultiplier`).

O Algoritmo de Avaliação (`HandEvaluator.cs`) suporta mãos de Poker tradicionais (High Card até Straight Flush).
* `Total Final` = `(Fichas Base + Valor Nominal das Cartas na Mão) * Multiplicador`.

### Consumíveis: Tarôs e Planetas
Um sistema flexível em `Consumables.cs` permite adquirir na Loja itens transmutadores do baralho:
- **Tarôs**: Podem alterar propriedades (`EnhancedCards`) em tempo real. O jogo suporta os efeitos visuais e de pontuação das cartas **Foil** (+50 Fichas), **Holographic** (+10 Mult) e **Polychrome** (x1.5 Mult) advindas de cartas como "The Magician", "The Lovers", e "The Chariot".
- **Planetas**: Aumentam o nível permanente e o *Base Chip/Mult* de mãos específicas de Poker. Júpiter bufando o Flush, Saturno focando no Straight, e Plutão upando os covardes de High Card.

### Sistema de Jokers (21 Exclusivos)
Você pode equipar até **5 Jokers** ao mesmo tempo. A coleção engloba agora 21 Jokers únicos configurados matematicamente no `ScoringEngine`.
Alguns destaques:
- **Jolly Joker / Mad Joker / Crazy Joker**: Focam aditivamente em Pares, Dois Pares e Straights.
- **Lusty, Wrathful, Gluttonous, Greedy Jokers**: Atrelados a recompensar pontuação por Naipes específicos na mão avaliada.
- **Supreme Joker**: Escalabilidade imensa. Oferece Multiplicação em cascata de `x3` pelo Straight.
- **Scary Face**: Recompensa generosa focada inteiramente em jogar os 'Face Cards' Reais (Valetes, Damas e Reis).

---

## 📁 Árvore de Diretórios (Visão Geral da Arquitetura C#)

O código se orgulha de aplicar forte `Separation of Concerns` sob as bênçãos estruturais do .NET:

```text
/Balatro101
│
├── Program.cs                 -> Ponto de entrada e Bootstrapping Engine-Window.
├── Assets/                    -> (Novo) Repositório de PNGs utilizados pelo jogo.
│
├── Game.Core/                 -> LÓGICA PURA MATEMÁTICA E INJEÇÃO DOS OBJETOS.
│   ├── AssetManager.cs        -> Carregador preguiçoso de texturas Singleton.
│   ├── AnimatableItem.cs      -> Classe base introduzindo Easing e Interpolação.
│   ├── Consumables.cs         -> Todas as regras de Cartas de Tarô e Planetas Ativos.
│   ├── Jokers.cs              -> Todos os 21 record classes/objetos passivos atuando no Score.
│   └── HandEvaluator.cs       -> Regras de Pôquer algorítmico estrito (Pure Function).
│
├── Game.Engine/               -> MESTRE DE MARIONETES E SISTEMAS.
│   ├── GameManager.cs         -> FSM e Gestão Financeira, Embaralhador, Target Score, Bosses.
│   ├── InputManager.cs        -> Adaptador e Pooler dos controles Mouse/Gamepad.
│   ├── UINavigator / UINode   -> Árvore de Decisão do uso de Setas/Gamepads na Tela.
│   └── ScoringEngine.cs       -> Ordem de Precedências para o ScoreAnimator usar de rascunho.
│
└── Game.UI/                   -> EVENTOS DE DOMÍNIO E RENDERIZAÇÃO ESTÉTICA (VIEW).
    ├── GameUI.cs              -> Loop Central. Responsável pela Tela de Jogo principal.
    ├── ScoreAnimator.cs       -> Avaliador Passo-a-Passo ("Ping" de Chips, Multiplier Pops).
    ├── ParticleSystem.cs      -> Sparkles, Explosões Red/Blue físicas 2D e Confetes por gravidade.
    ├── FloatingText.cs        -> Balística parabólica dos números saltando das Cartas para o HUD.
    └── RendererUtils.cs       -> Desenhista especializado dos Cards baseados no AssetManager.
```

---

## 🚀 Como Rodar
Certifique-se de ter o `.NET 8 SDK` e o MS-Build rodando na plataforma nativa. A engine do projeto se apoia no `Raylib-cs` e extrai toda a leveza do C++. 
Abra o Bash no diretório do `.csproj` e execute a construção:
```bash
dotnet run
```

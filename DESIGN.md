# Дизайн и стили NesExtractor

## Обзор

NesExtractor использует **Fluent Design System** от Avalonia с кастомными стилями для современного и приятного UI.

## Структура стилей

```
src/NesExtractor/Styles/
├── TabStyles.axaml      # Стили для табов
└── CommonStyles.axaml   # Общие стили (кнопки, панели)
```

## Цветовая схема

Приложение следует системной теме (светлая/темная):
- **Акцентный цвет**: Системный акцентный цвет (обычно синий)
- **Фон**: Адаптивный (белый/темно-серый)
- **Текст**: Адаптивный с различными уровнями контраста
- **Графика**: Темно-серый фон (#202020) для контраста с тайлами

## Компоненты

### 1. Табы (TabStyles.axaml)

**Характеристики:**
- Высота: 40px
- Отступы: 16px горизонтально, 8px вертикально
- Нижняя граница: 2px (прозрачная в неактивном состоянии)
- Шрифт: 13px (Normal), 13px (SemiBold для активного)

**Состояния:**

#### Неактивный таб
```
Background: Transparent
Foreground: Medium (серый)
BorderBrush: Transparent
```

#### Hover (наведение)
```
Background: List Low (светло-серый)
Foreground: Base High (яркий)
```

#### Активный таб
```
Background: Alt High (основной фон)
BorderBrush: Accent Color (синий)
Foreground: Base High (яркий)
FontWeight: SemiBold
```

**Кнопка закрытия:**
- Размер: 20×20px
- Округление: 3px
- Прозрачный фон
- Hover: серый фон с переходом
- Символ: × (U+00D7)

**Иконка файла:**
- Размер: 14×14px
- Отступ справа: 8px
- Иконка документа (Material Design)

**Анимации:**
- Background: 150ms
- BorderBrush: 150ms
- Foreground: 150ms

### 2. Кнопки

#### Export Button (Кнопки экспорта)
```css
Padding: 16px 10px
FontWeight: SemiBold
CornerRadius: 6px
Background: Accent Color
Foreground: Accent Light 3
Transitions: 150ms
```

**Эффекты:**
- **Hover**: Darker accent + scale(1.02)
- **Pressed**: scale(0.98)
- **Disabled**: Opacity 50%

#### Zoom Button (Кнопки зума)
```css
MinWidth: 36px
MinHeight: 32px
CornerRadius: 4px
FontWeight: SemiBold
```

**Эффекты:**
- **Hover**: Medium gray background

### 3. Информационные панели

```css
Background: Alt High
BorderBrush: Medium Low
BorderThickness: 1px
CornerRadius: 8px
Padding: 16px
```

### 4. Empty State

**Структура:**
- Большая иконка (80×80px, 50% opacity)
- Заголовок (22px, SemiBold)
- Описание (14px, Medium brush)
- Информационная панель с галочками
- Большая кнопка действия
- Подсказка по горячей клавише

### 5. Разделители

```css
Height: 1px
Background: Medium Low
Margin: 8px vertical
```

### 6. Тултипы

```css
Background: Chrome Medium Low
Padding: 8px 6px
CornerRadius: 4px
BorderThickness: 1px
BorderBrush: Medium Low
```

## Типографика

### Размеры шрифтов

| Элемент | Размер | Вес |
|---------|--------|-----|
| Заголовок секции | 16-18px | SemiBold |
| Заголовок приветствия | 22px | SemiBold |
| Основной текст | 13-14px | Normal |
| Метки полей | 13px | SemiBold |
| Подсказки | 11-12px | Normal |

### Шрифты

- **Основной**: Segoe UI (Windows), Inter (Linux/macOS)
- **Моноширинный**: Consolas, JetBrains Mono (для будущих hex-значений)

## Отступы и сетка

Базовая сетка: **8px**

### Стандартные отступы
- XS: 4px
- S: 8px
- M: 12px
- L: 16px
- XL: 20px
- XXL: 24px

### Примеры использования
- Внутренние отступы панелей: 16-20px
- Отступы между элементами: 8-12px
- Отступы между секциями: 16-24px

## Адаптивность

### Минимальные размеры окна
- Ширина: 800px
- Высота: 600px

### Рекомендуемые размеры
- Ширина: 1000px
- Высота: 700px

### Панели
- Левая (информация): 30% (min 240px)
- Правая (графика): 70%

## Анимации и переходы

### Стандартные длительности
- Быстро: 100ms (pressed effects)
- Средне: 150ms (hover, background)
- Медленно: 200ms (сложные переходы)

### Используемые переходы
- `BrushTransition` - для цветов
- `TransformOperationsTransition` - для масштабирования
- `DoubleTransition` - для opacity

### Примеры
```xaml
<Setter Property="Transitions">
    <Transitions>
        <BrushTransition Property="Background" Duration="0:0:0.15" />
        <TransformOperationsTransition Property="RenderTransform" Duration="0:0:0.1" />
    </Transitions>
</Setter>
```

## Иконки

Используются Material Design Icons в формате PathIcon:
- Документ/Файл
- Закрыть (×)
- Информация (?)
- Графика (игровой контроллер)
- Экспорт (стрелка вниз)
- Папка

### Размеры иконок
- В табах: 14×14px
- В кнопках: 16-20px
- В empty state: 80×80px
- В информационных блоках: 20-24px

## Интерактивные элементы

### Feedback система

**Кнопки:**
1. Normal → Hover: изменение фона + cursor pointer
2. Hover → Pressed: scale(0.98)
3. Pressed → Normal: возврат масштаба

**Табы:**
1. Normal → Hover: подсветка фона
2. Hover → Active: accent border снизу
3. Active: жирный шрифт + accent border

**Hover delays:**
- Кнопки: без задержки
- Табы: без задержки
- Тултипы: 400ms задержка появления

## Best Practices

### Цвета
✅ **Используйте**: `{DynamicResource SystemControlXxx}`  
❌ **Не используйте**: Жестко заданные #RRGGBB цвета

### Отступы
✅ **Используйте**: Кратные 4 или 8  
❌ **Не используйте**: Нечетные значения (кроме 1px для границ)

### Шрифты
✅ **Используйте**: 12-22px для UI элементов  
❌ **Не используйте**: Размеры > 24px (кроме специальных случаев)

### Анимации
✅ **Используйте**: 100-200ms для UI  
❌ **Не используйте**: > 300ms (слишком медленно)

## Доступность

- **Контраст**: Все текстовые элементы имеют достаточный контраст
- **Размеры**: Интерактивные элементы минимум 32×32px
- **Клавиатура**: Полная навигация через Tab
- **Скринридеры**: Все кнопки имеют тултипы

## Поддержка тем

Приложение автоматически следует системной теме:
```csharp
RequestedThemeVariant="Default"
```

Доступные варианты:
- `Default` - системная (рекомендуется)
- `Light` - светлая
- `Dark` - темная

## Будущие улучшения

- [ ] Кастомная цветовая схема для NES палитр
- [ ] Анимация при открытии/закрытии табов
- [ ] Smooth scroll для графики
- [ ] Drag & Drop файлов с визуальным feedback
- [ ] Прогресс-бар для экспорта больших файлов


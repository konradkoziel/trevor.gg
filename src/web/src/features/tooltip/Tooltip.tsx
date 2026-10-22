import {
  cloneElement,
  useId,
  useLayoutEffect,
  useRef,
  useState,
  type FocusEvent,
  type MouseEvent,
  type ReactElement,
} from 'react'
import { createPortal } from 'react-dom'
import { tooltipPosition } from './tooltipPosition'

interface TriggerProps {
  onMouseEnter?: (e: MouseEvent<HTMLElement>) => void
  onMouseLeave?: (e: MouseEvent<HTMLElement>) => void
  onFocus?: (e: FocusEvent<HTMLElement>) => void
  onBlur?: (e: FocusEvent<HTMLElement>) => void
  'aria-describedby'?: string
}

interface Props {
  title: string
  // e.g. an item's price, under the name
  detail?: string
  text?: string
  children: ReactElement<TriggerProps>
}

// Shows at once on hover or keyboard focus, unlike a title attribute. Rendered into <body> with a fixed position,
// so scrolling containers (the skill order table) don't cut it off
export function Tooltip({ title, detail, text, children }: Props) {
  const id = useId()
  const [anchor, setAnchor] = useState<DOMRect>()
  const [position, setPosition] = useState<{ left: number; top: number }>()
  const tooltipRef = useRef<HTMLDivElement>(null)

  useLayoutEffect(() => {
    if (!anchor || !tooltipRef.current) return
    const { width, height } = tooltipRef.current.getBoundingClientRect()
    setPosition(tooltipPosition(anchor, { width, height }, { width: window.innerWidth, height: window.innerHeight }))
  }, [anchor])

  const show = (e: MouseEvent<HTMLElement> | FocusEvent<HTMLElement>) =>
    setAnchor(e.currentTarget.getBoundingClientRect())
  const hide = () => {
    setAnchor(undefined)
    setPosition(undefined)
  }

  const trigger = cloneElement(children, {
    onMouseEnter: show,
    onMouseLeave: hide,
    onFocus: show,
    onBlur: hide,
    'aria-describedby': anchor ? id : undefined,
  })

  return (
    <>
      {trigger}
      {anchor &&
        createPortal(
          <div
            ref={tooltipRef}
            id={id}
            role="tooltip"
            className="tooltip"
            style={position ? { left: position.left, top: position.top } : { visibility: 'hidden' }}
          >
            <strong className="tooltip-title">{title}</strong>
            {detail && <span className="tooltip-detail">{detail}</span>}
            {text && <p className="tooltip-text">{text}</p>}
          </div>,
          document.body,
        )}
    </>
  )
}

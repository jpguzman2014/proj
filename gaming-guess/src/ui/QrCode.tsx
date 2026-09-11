import { useMemo } from 'react';
import qrcode from 'qrcode-generator';

/**
 * The QR code is generated in the browser. No image service is contacted, so
 * the join link never leaves the device that made it.
 */
export default function QrCode({ value, label }: { value: string; label: string }) {
  const svg = useMemo(() => {
    const qr = qrcode(0, 'M');
    qr.addData(value);
    qr.make();
    return qr.createSvgTag({ cellSize: 4, margin: 0, scalable: true });
  }, [value]);

  return (
    <div
      className="qr-code"
      role="img"
      aria-label={label}
      dangerouslySetInnerHTML={{ __html: svg }}
    />
  );
}

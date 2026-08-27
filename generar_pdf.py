#!/usr/bin/env python3
"""PDF del estado real de UMBRA - todo el sistema funcionando, solo faltan imagenes."""
from reportlab.lib.pagesizes import A4
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, HRFlowable, Table, TableStyle
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import mm, inch
from reportlab.lib.colors import HexColor

doc = SimpleDocTemplate(
    '/Users/andrew/ProyectosPersonales/Umbra/Avance_Umbra.pdf',
    pagesize=A4, topMargin=0.9*inch, bottomMargin=0.6*inch,
    leftMargin=1*inch, rightMargin=1*inch)

sT = ParagraphStyle('STi', fontSize=28, leading=34, textColor='#1a1a2e', alignment=1, spaceAfter=8)
sU = ParagraphStyle('SUB', fontSize=12, leading=16, textColor='#888888', alignment=1, spaceAfter=40)
sH1 = ParagraphStyle('S1', fontSize=17, leading=23, textColor='#1a1a2e', spaceBefore=26, spaceAfter=10, fontName='Helvetica-Bold')
sH2 = ParagraphStyle('S2', fontSize=13, leading=18, textColor='#d35400', spaceBefore=16, spaceAfter=6, fontName='Helvetica-Bold')
sB = ParagraphStyle('SBd', fontSize=10.5, leading=15, textColor='#2c3e50')
sL = ParagraphStyle('SLi', leftIndent=18, fontSize=10.5, leading=16, textColor='#2c3e50')

E = []

# PORTADA
E.append(Paragraph('PROYECTO UMBRA', sT))
txt_sub = 'Estado real del juego - Unidad 2'
E.append(Paragraph(txt_sub, sU))
E.append(HRFlowable(width='70%', thickness=2, color='#1a1a2e', spaceAfter=40))

# SECCION 1: LO QUE YA FUNCIONA
E.append(Paragraph('LO QUE YA ESTA COMPLETO - SISTEMA DE JUEGO FUNCIONAL', sH1))

# Nota clave
txt_nota = ('<b>Nota importante:</b><br/>')
txt_nota += 'El script <i>UmbraLevelBuilder.cs</i> genera TODOS los 5 niveles desde el menu de Unity<br/>'
txt_nota += '(Umbra -> Construir todos los niveles) con TODO el sistema de juego funcionando.<br/><br/>'
txt_nota += '<b>Lo unico que falta son las imagenes/textura sobre cada objeto para que se vea.</b>'
E.append(Paragraph(txt_nota, sB))

# Tabla: Sistemas completados
tb1 = Table([
    ['Sistema de Juego', 'Estado'],
    ['5 Niveles con geometria completa.', 'OK'],
    ['Personaje (movimiento)', 'OK - Ya funciona'],
    ['Camera que sigue al jugador', 'OK - Suavemente configurada'],
    ['Plataformas dinamicas (3 tipos)', 'OK - Funcionan todas'],
    ['Enemigos con IA de patrol', 'OK - Implementados en niveles 2-5'],
    ['Checkpoints y zona meta', 'OK - Sistema completo'],
    ['Zonas hazard (peligro)', 'OK - Triggers activos']
], colWidths=[100*mm, 87*mm])

tb1.setStyle(TableStyle([
    ('BACKGROUND', (0,0), (-1,0), '#1a1a2e'),
    ('TEXTCOLOR', (0,0), (-1,-1), '#ffffff'),
    ('FONTNAME', (0,0), (-1,0), 'Helvetica-Bold'),
    ('FONTSIZE', (0,0), (-1,0), 10),
    ('FONTNAME', (0,1), (-1,-1), 'Helvetica'),
    ('FONTSIZE', (0,1), (-1,-1), 9.5),
    ('BACKGROUND', (0,1), (-1,-1), '#ffffff'),
    ('GRID', (0,0), (-1,-1), 0.8, '#dddddd'),
    ('VALIGN', (0,0), (-1,-1), 'MIDDLE'),
    ('TOPPADDING', (0,0), (-1,-1), 5),
    ('BOTTOMPADDING', (0,0), (-1,-1), 5)
]))
E.append(tb1)

# SECCION 2: SOLO FALTA TEXTURA
E.append(Spacer(1, 28))
E.append(HRFlowable(width='95%', thickness=1.2, color='#c0392b', spaceAfter=24))
E.append(Paragraph('SOLO FALTAN IMAGENES/TEXTURAS - TODO FUNCIONA MEKANICAMENTE', sH1))

# Tabla detalle
tb2 = Table([
    ['', '', ''],
    ['Que ya EXISTE (mecanicamente)', 'Estado actual de lo que hay', 'Solo falta poner imagenes/textura'],
    ['', '', ''],
    ['Los objetos existen en el mapa con colliders funcionales.',
     '- El personaje YA choca correctamente con las plataformas',
     '- Poner PNG sobre cada caja vacia para ver textura'
    ],
    ['', '', ''],
    ['Sistema de juego completo funcionando:',
     '- Builder crea 5 niveles con todo listo',
     '- Solo necesita texturas para verse visualmente bien'
    ]
], colWidths=[42*mm, 60*mm, 65*mm])

tb2.setStyle(TableStyle([
    ('BACKGROUND', (0,1), (-1,1), '#d35400'),
    ('TEXTCOLOR', (0,1), (-1,1), '#ffffff'),
    ('FONTNAME', (0,1), (-1,1), 'Helvetica-Bold'),
    ('FONTSIZE', (0,1), (-1,1), 9),
    ('BACKGROUND', (0,3), (-1,3), '#ffffff'),
    ('BACKGROUND', (0,5), (-1,5), '#ffffff'),
    ('GRID', (0,0), (-1,-1), 0.6, '#eeeeee'),
    ('VALIGN', (0,0), (-1,-1), 'TOP'),
    ('TOPPADDING', (0,0), (-1,-1), 5),
    ('BOTTOMPADDING', (0,0), (-1,-1), 5)
]))
E.append(tb2)

# Resumen final
E.append(Spacer(1, 30))
txt_res = '<b>Resumen:</b> El juego esta mecanicamente completo. '
txt_res += 'El builder genera los 5 niveles con todo funcionando (personaje, plataformas dinamicas, '
txt_res += 'enemigos de patrol, checkpoints y zonas meta). Solo faltan texturas sobre cada objeto.'

E.append(HRFlowable(width='60%', thickness=2, color='#1a1a2e'))
E.append(Paragraph(txt_res, sB))

# Crear PDF
doc.build(E)

import os
pf = '/Users/andrew/ProyectosPersonales/Umbra/Avance_Umbra.pdf'
sz = os.path.getsize(pf)
print('PDF creado exitosamente!')
print('  Path: ' + pf)
print('  Tamano: ' + str(sz) + ' bytes')

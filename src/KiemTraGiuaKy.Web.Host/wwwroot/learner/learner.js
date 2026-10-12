// Learner Client Application - Google Stitch TOEIC UI
document.addEventListener('DOMContentLoaded', () => {
  const viewExamsList = document.getElementById('viewExamsList');
  const viewTakingExam = document.getElementById('viewTakingExam');
  const viewExamResult = document.getElementById('viewExamResult');
  const examsGrid = document.getElementById('examsGrid');
  const timerDisplay = document.getElementById('timerDisplay');
  const btnSubmitExam = document.getElementById('btnSubmitExam');
  const btnExitExam = document.getElementById('btnExitExam');
  const btnResultBackToHome = document.getElementById('btnResultBackToHome');
  const btnBackToExamsList = document.getElementById('btnBackToExamsList');
  const btnReviewAttemptAnswers = document.getElementById('btnReviewAttemptAnswers');

  const btnPrevQuestion = document.getElementById('btnPrevQuestion');
  const btnNextQuestion = document.getElementById('btnNextQuestion');
  const btnOpenPalette = document.getElementById('btnOpenPalette');
  const paletteModal = document.getElementById('paletteModal');
  const btnClosePalette = document.getElementById('btnClosePalette');
  const paletteGrid = document.getElementById('paletteGrid');
  const btnBookmark = document.getElementById('btnBookmark');

  const hintAccordion = document.getElementById('hintAccordion');
  const hintContentBox = document.getElementById('hintContentBox');
  const hintChevron = document.getElementById('hintChevron');

  let currentAttemptId = null;
  let currentExam = null;
  let flatQuestions = [];
  let currentIndex = 0;
  let timerInterval = null;
  let remainingSeconds = 75 * 60;
  let userAnswersMap = {}; // questionId -> selectedOption (A/B/C/D)
  let bookmarkedQuestions = new Set();
  let startTimeStamp = null;

  // Initialize
  loadAvailableExams();

  // Navigation events
  if (btnExitExam) {
    btnExitExam.addEventListener('click', () => {
      if (confirm('Bạn có muốn tạm dừng và thoát ra ngoài không?')) {
        showView(viewExamsList);
        clearInterval(timerInterval);
        loadAvailableExams();
      }
    });
  }

  if (btnResultBackToHome) {
    btnResultBackToHome.addEventListener('click', () => {
      showView(viewExamsList);
      loadAvailableExams();
    });
  }

  if (btnBackToExamsList) {
    btnBackToExamsList.addEventListener('click', () => {
      showView(viewExamsList);
      loadAvailableExams();
    });
  }

  // Submit
  if (btnSubmitExam) {
    btnSubmitExam.addEventListener('click', () => {
      const answeredCount = Object.keys(userAnswersMap).length;
      const total = flatQuestions.length;
      if (confirm(`Bạn đã hoàn thành ${answeredCount}/${total} câu hỏi. Bạn có chắc chắn muốn nộp bài ngay bây giờ?`)) {
        finishAttempt();
      }
    });
  }

  // Palette modal
  if (btnOpenPalette) {
    btnOpenPalette.addEventListener('click', () => {
      renderPalette();
      paletteModal.style.display = 'flex';
    });
  }
  if (btnClosePalette) {
    btnClosePalette.addEventListener('click', () => {
      paletteModal.style.display = 'none';
    });
  }

  // Bookmark toggle
  if (btnBookmark) {
    btnBookmark.addEventListener('click', () => {
      const q = flatQuestions[currentIndex];
      if (!q) return;
      if (bookmarkedQuestions.has(q.id)) {
        bookmarkedQuestions.delete(q.id);
        btnBookmark.classList.remove('active');
      } else {
        bookmarkedQuestions.add(q.id);
        btnBookmark.classList.add('active');
      }
    });
  }

  // Accordion Hint toggle
  if (hintAccordion) {
    hintAccordion.addEventListener('click', () => {
      const isVisible = hintContentBox.style.display === 'block';
      hintContentBox.style.display = isVisible ? 'none' : 'block';
      hintChevron.style.transform = isVisible ? 'rotate(0deg)' : 'rotate(180deg)';
    });
  }

  // Question navigation
  if (btnPrevQuestion) {
    btnPrevQuestion.addEventListener('click', () => {
      if (currentIndex > 0) {
        currentIndex--;
        displayQuestion(currentIndex);
      }
    });
  }

  if (btnNextQuestion) {
    btnNextQuestion.addEventListener('click', () => {
      if (currentIndex < flatQuestions.length - 1) {
        currentIndex++;
        displayQuestion(currentIndex);
      } else {
        btnSubmitExam.click();
      }
    });
  }

  function showView(viewToShow) {
    viewExamsList.style.display = 'none';
    viewTakingExam.style.display = 'none';
    viewExamResult.style.display = 'none';
    viewToShow.style.display = viewToShow === viewTakingExam ? 'flex' : 'block';
  }

  // ── 1. LOAD AVAILABLE EXAMS ──
  async function loadAvailableExams() {
    examsGrid.innerHTML = `
      <div style="text-align:center; padding:3rem; color:var(--text-muted);">
        <i class="fa-solid fa-spinner fa-spin"></i> Đang tải danh sách đề thi...
      </div>
    `;

    try {
      const res = await fetch('/api/mobile/exams');
      if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Mã lỗi ${res.status}: ${res.statusText || errorText || 'Không thể kết nối đến máy chủ'}`);
      }
      const raw = await res.json();
      const exams = raw.result !== undefined ? raw.result : raw;

      document.getElementById('availableExamsCount').textContent = `${exams.length} bộ đề`;

      if (exams.length === 0) {
        examsGrid.innerHTML = `
          <div style="background:white; border:1px solid var(--border); border-radius:12px; padding:32px 20px; text-align:center;">
            <div style="font-size:36px; color:#cbd5e1; margin-bottom:12px;"><i class="fa-regular fa-folder-open"></i></div>
            <div style="font-size:15px; font-weight:700; color:var(--text-dark); margin-bottom:4px;">Chưa có đề thi nào được xuất bản</div>
            <div style="font-size:13px; color:var(--text-muted); margin-bottom:16px;">Vui lòng truy cập trang Quản trị (Admin) để bấm nút "Xuất bản" đề thi vừa tải lên.</div>
            <a href="/admin/index.html" style="display:inline-flex; align-items:center; gap:6px; background:#2563eb; color:white; padding:8px 16px; border-radius:8px; font-size:13px; font-weight:600; text-decoration:none;">
              <i class="fa-solid fa-arrow-left"></i> Đến trang Quản trị Admin
            </a>
          </div>
        `;
        return;
      }

      examsGrid.innerHTML = exams.map((exam, i) => `
        <div style="background:white; border:1.5px solid var(--border); border-radius:16px; padding:18px; display:flex; flex-direction:column; gap:10px; cursor:pointer; transition:all 0.15s;" 
             onclick="startExam(${exam.id})"
             onmouseover="this.style.borderColor='#93c5fd'; this.style.transform='translateY(-2px)'"
             onmouseout="this.style.borderColor='var(--border)'; this.style.transform='none'">
          <div style="display:flex; justify-content:space-between; align-items:center;">
            <span style="background:#eff6ff; color:#1d4ed8; font-size:11px; font-weight:700; padding:3px 10px; border-radius:999px;">
              Đề số ${i + 1} • TOEIC Reading
            </span>
            <span style="font-size:12px; color:var(--text-muted); display:flex; align-items:center; gap:4px;">
              <i class="fa-regular fa-clock"></i> ${exam.durationMinutes} phút
            </span>
          </div>

          <div>
            <div style="font-size:15px; font-weight:800; color:var(--text-dark); margin-bottom:4px;">
              ${escapeHtml(exam.title)}
            </div>
            <div style="font-size:12px; color:var(--text-muted); line-height:1.4;">
              ${escapeHtml(exam.description || 'Đề thi đọc hiểu TOEIC')}
            </div>
          </div>

          <div style="display:flex; justify-content:space-between; align-items:center; padding-top:8px; border-top:1px solid var(--border-light);">
            <span style="font-size:13px; font-weight:700; color:#475569;">
              <i class="fa-regular fa-circle-question" style="color:#2563eb;"></i> ${exam.totalQuestions} câu hỏi
            </span>
            <button style="background:#2563eb; color:white; border:none; padding:8px 16px; border-radius:8px; font-size:13px; font-weight:700; display:flex; align-items:center; gap:6px; cursor:pointer;">
              Vào làm bài <i class="fa-solid fa-chevron-right"></i>
            </button>
          </div>
        </div>
      `).join('');

    } catch (err) {
      examsGrid.innerHTML = `
        <div style="background:#fef2f2; border:1px solid #fecaca; color:#dc2626; border-radius:12px; padding:16px; font-size:13px;">
          <strong>Lỗi tải danh sách đề thi:</strong> ${escapeHtml(err.message)}
          <div style="margin-top:8px; font-size:12px; color:#4b5563;">
            Gợi ý: Hãy đảm bảo máy chủ backend ASP.NET đang chạy và bạn đã bấm nút khởi động lại máy chủ nếu vừa cập nhật code.
          </div>
        </div>
      `;
    }
  }

  // ── 2. START EXAM ATTEMPT ──
  window.startExam = async function(examId) {
    try {
      // 1. Khởi tạo lượt thi
      const startRes = await fetch('/api/mobile/attempts/start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ examId: examId })
      });

      // Đọc body chỉ MỘT lần (stream chỉ đọc được 1 lần)
      const startRaw = await startRes.json().catch(() => ({}));

      if (!startRes.ok) {
        throw new Error(startRaw.error?.message || startRaw.message || 'Không thể khởi tạo lượt thi. Đề thi phải ở trạng thái ĐÃ XUẤT BẢN.');
      }

      const attemptData = startRaw.result !== undefined ? startRaw.result : startRaw;
      currentAttemptId = attemptData.attemptId;
      remainingSeconds = attemptData.remainingSeconds || (attemptData.durationMinutes * 60);
      startTimeStamp = new Date();

      // 2. Tải nội dung câu hỏi
      const contentRes = await fetch(`/api/mobile/exams/${examId}`);
      if (!contentRes.ok) throw new Error('Không thể tải nội dung đề thi');
      const contentRaw = await contentRes.json();
      currentExam = contentRaw.result !== undefined ? contentRaw.result : contentRaw;

      // 3. Gom tất cả câu hỏi: Standalone (Part 5) + Groups (Part 6 & 7)
      flatQuestions = [];
      userAnswersMap = {};
      bookmarkedQuestions.clear();

      // Part 5 Standalone
      if (currentExam.standaloneQuestions && currentExam.standaloneQuestions.length > 0) {
        currentExam.standaloneQuestions.forEach(q => {
          flatQuestions.push({
            ...q,
            partNumber: q.partNumber || 5,
            instruction: 'Incomplete Sentences',
            passageItems: []
          });
        });
      }

      // Part 6 & 7 Groups
      if (currentExam.groups && currentExam.groups.length > 0) {
        currentExam.groups.forEach(group => {
          const passageItems = group.passageItems || group.passages || [];
          if (group.questions) {
            group.questions.forEach(q => {
              flatQuestions.push({
                ...q,
                partNumber: group.partNumber,
                instruction: group.instruction,
                passageItems: passageItems
              });
            });
          }
        });
      }

      flatQuestions.sort((a, b) => a.questionNumber - b.questionNumber);

      if (flatQuestions.length === 0) {
        alert('Đề thi này chưa có câu hỏi nào!');
        return;
      }

      currentIndex = 0;
      showView(viewTakingExam);
      displayQuestion(currentIndex);
      startTimer();

    } catch (err) {
      alert(`Lỗi: ${err.message}`);
    }
  };

  function normalizeOption(val) {
    if (val === null || val === undefined) return '';
    if (val === 1 || val === '1' || val === 'A' || val === 'a') return 'A';
    if (val === 2 || val === '2' || val === 'B' || val === 'b') return 'B';
    if (val === 3 || val === '3' || val === 'C' || val === 'c') return 'C';
    if (val === 4 || val === '4' || val === 'D' || val === 'd') return 'D';
    return String(val).replace(/[^A-Za-z]/g, '').toUpperCase();
  }

  // ── 3. DISPLAY QUESTION (MOCKUP 2 MATCH) ──
  function displayQuestion(idx) {
    const q = flatQuestions[idx];
    if (!q) return;

    const total = flatQuestions.length;
    const qText = q.content || q.text || '';

    // Header info
    document.getElementById('takingPartTitle').textContent = `Đề số 1 • Part ${q.partNumber}`;
    const partSubNames = {
      5: 'Incomplete Sentences',
      6: 'Text Completion',
      7: 'Reading Comprehension'
    };
    document.getElementById('takingPartSub').textContent = partSubNames[q.partNumber] || 'Reading';

    // Part tag & progress text
    const partInstructions = {
      5: 'Part 5 Điền từ vào câu',
      6: 'Part 6 Hoàn thành đoạn văn',
      7: 'Part 7 Đọc hiểu thông tin'
    };
    document.getElementById('currentPartTag').textContent = partInstructions[q.partNumber] || `Part ${q.partNumber}`;

    const answeredCount = Object.keys(userAnswersMap).length;
    const progressPercent = Math.round(((idx + 1) / total) * 100);

    document.getElementById('questionProgressText').innerHTML = `Câu ${q.questionNumber}/${total} <span>(${progressPercent}%)</span>`;
    document.getElementById('progressLineFill').style.width = `${progressPercent}%`;

    document.getElementById('currentQuestionNumBadge').textContent = `Câu hỏi ${q.questionNumber}`;
    document.getElementById('btnPaletteLabel').textContent = `Câu ${q.questionNumber}/${total}`;

    // Bookmark status
    if (bookmarkedQuestions.has(q.id)) {
      btnBookmark.classList.add('active');
    } else {
      btnBookmark.classList.remove('active');
    }

    // Passage box for Part 6/7
    const passageBox = document.getElementById('passageContainer');
    const passages = q.passageItems || q.passages || [];
    if (passages && passages.length > 0) {
      passageBox.style.display = 'block';
      passageBox.innerHTML = passages.map(p => {
        if (p.type === 2 && p.imageUrl) {
          return `<div style="text-align:center; margin:8px 0;"><img src="${p.imageUrl}" style="max-width:100%; border-radius:8px;"></div>`;
        }
        return `<div>${escapeHtml(p.textContent || p.text || '')}</div>`;
      }).join('<hr style="margin:10px 0; border:none; border-top:1px dashed #cbd5e1;">');
    } else {
      passageBox.style.display = 'none';
      passageBox.innerHTML = '';
    }

    // Question text with highlight blank (e.g. ------ -> fill-blank)
    const textHtml = escapeHtml(qText).replace(/_{3,}|-{3,}/g, '<span class="fill-blank">------</span>');
    document.getElementById('questionTextBox').innerHTML = `
      <strong class="q-number">${q.questionNumber}.</strong> ${textHtml}
    `;

    // Render 4 options A, B, C, D
    const optionsContainer = document.getElementById('optionsContainer');
    const selectedAnswer = normalizeOption(userAnswersMap[q.id]);

    optionsContainer.innerHTML = (q.options || []).map(opt => {
      const optLetter = normalizeOption(opt.label);
      const isSelected = selectedAnswer && selectedAnswer === optLetter;
      const optText = opt.content || opt.text || '';
      return `
        <div class="option-item ${isSelected ? 'selected' : ''}" onclick="selectAnswer(${q.id}, '${optLetter}')">
          <div class="opt-circle">${optLetter}</div>
          <div class="opt-text">${escapeHtml(optText)}</div>
          <div class="opt-status-tag">
            <span>Đã chọn</span>
            <i class="fa-regular fa-circle-check"></i>
          </div>
        </div>
      `;
    }).join('');

    // Prev / Next button states
    btnPrevQuestion.disabled = idx === 0;
    btnPrevQuestion.style.opacity = idx === 0 ? '0.5' : '1';

    if (idx === total - 1) {
      btnNextQuestion.innerHTML = `<span>Hoàn thành</span> <i class="fa-solid fa-check"></i>`;
    } else {
      btnNextQuestion.innerHTML = `<span>Câu sau</span> <i class="fa-solid fa-chevron-right"></i>`;
    }
  }

  // Option selection / Deselect handler
  window.selectAnswer = async function(questionId, optionLabel) {
    const letter = normalizeOption(optionLabel);
    const optionEnumMap = { 'A': 1, 'B': 2, 'C': 3, 'D': 4 };
    let newSelection = letter;

    if (normalizeOption(userAnswersMap[questionId]) === letter) {
      // Bấm lần nữa để hủy chọn
      delete userAnswersMap[questionId];
      newSelection = null;
    } else {
      userAnswersMap[questionId] = letter;
    }

    displayQuestion(currentIndex);

    // Lưu câu trả lời theo thời gian thực tới backend
    if (currentAttemptId) {
      try {
        await fetch('/api/mobile/attempts/answer', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            attemptId: currentAttemptId,
            questionId: questionId,
            selectedOption: newSelection ? (optionEnumMap[newSelection] || null) : null
          })
        });
      } catch (e) {
        console.warn('Không thể lưu câu trả lời tự động:', e);
      }
    }
  };

  // ── 4. PALETTE NAVIGATION ──
  function renderPalette() {
    paletteGrid.innerHTML = flatQuestions.map((q, idx) => {
      const isAnswered = !!userAnswersMap[q.id];
      const isCurrent = idx === currentIndex;
      const isBookmarked = bookmarkedQuestions.has(q.id);

      let cls = 'palette-btn';
      if (isCurrent) cls += ' current';
      else if (isAnswered) cls += ' answered';
      if (isBookmarked) cls += ' bookmarked';

      return `
        <button class="${cls}" onclick="jumpToQuestion(${idx})">
          ${q.questionNumber}
        </button>
      `;
    }).join('');
  }

  window.jumpToQuestion = function(idx) {
    currentIndex = idx;
    paletteModal.style.display = 'none';
    displayQuestion(currentIndex);
  };

  // ── 5. COUNTDOWN TIMER ──
  function startTimer() {
    clearInterval(timerInterval);
    updateTimerText();

    timerInterval = setInterval(() => {
      remainingSeconds--;
      if (remainingSeconds <= 0) {
        clearInterval(timerInterval);
        alert('Hết giờ làm bài! Hệ thống đang tự động nộp bài.');
        finishAttempt();
        return;
      }
      updateTimerText();
    }, 1000);
  }

  function updateTimerText() {
    const mins = Math.floor(remainingSeconds / 60);
    const secs = remainingSeconds % 60;
    timerDisplay.textContent = `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
  }

  // ── 6. SUBMIT ATTEMPT & SHOW RESULT (MOCKUP 3 MATCH) ──
  async function finishAttempt() {
    clearInterval(timerInterval);

    // Hiển thị trạng thái đang nộp bài trên nút
    if (btnSubmitExam) {
      btnSubmitExam.disabled = true;
      btnSubmitExam.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> <span>Đang chấm điểm...</span>';
    }

    try {
      const submitRes = await fetch('/api/mobile/attempts/finish', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ attemptId: currentAttemptId })
      });

      // Đọc body chỉ MỘT lần
      const submitRaw = await submitRes.json().catch(() => ({}));

      if (!submitRes.ok) {
        throw new Error(submitRaw.error?.message || submitRaw.message || 'Lỗi khi nộp bài');
      }

      const result = submitRaw.result !== undefined ? submitRaw.result : submitRaw;
      displayResult(result);

    } catch (err) {
      alert(`Lỗi nộp bài: ${err.message}`);
      // Khôi phục nút nộp bài nếu lỗi
      if (btnSubmitExam) {
        btnSubmitExam.disabled = false;
        btnSubmitExam.innerHTML = '<i class="fa-regular fa-square-check"></i> <span>Nộp bài</span>';
      }
    }
  }

  function displayResult(res) {
    showView(viewExamResult);

    const summary = res.summary || res;
    const total = summary.totalQuestions || flatQuestions.length || 100;
    const correct = summary.correctCount || 0;
    const wrong = summary.wrongCount || 0;
    const skipped = summary.skippedCount !== undefined ? summary.skippedCount : (total - (correct + wrong));
    const accuracy = total > 0 ? Math.round((correct / total) * 100) : 0;
    const scaledScore = summary.scaledScore !== undefined ? summary.scaledScore : (correct * 5);

    // Ước tính TOEIC Scaled Score
    document.getElementById('resScaledScore').textContent = scaledScore;
    document.getElementById('resAccuracyBadge').textContent = `${accuracy}%`;

    // 3 Quick cards
    document.getElementById('quickCorrectVal').textContent = String(correct).padStart(2, '0');
    document.getElementById('quickWrongVal').textContent = String(wrong).padStart(2, '0');
    document.getElementById('quickSkippedVal').textContent = String(skipped).padStart(2, '0');

    // Breakdown
    document.getElementById('resCorrectDetail').textContent = `${correct} câu (${Math.round((correct/total)*100)}%)`;
    document.getElementById('resWrongDetail').textContent = `${wrong} câu (${Math.round((wrong/total)*100)}%)`;
    document.getElementById('resSkippedDetail').textContent = `${skipped} câu (${Math.round((skipped/total)*100)}%)`;

    // Time spent
    const timeSpentSeconds = summary.timeSpentSeconds || Math.max(0, (75 * 60) - remainingSeconds);
    const em = Math.floor(timeSpentSeconds / 60);
    const es = timeSpentSeconds % 60;
    document.getElementById('resTimeSpent').textContent = `Thời gian: ${em}p ${es}s / 75p`;

    // Donut SVG circumference calculation (251)
    const donutGreen = document.getElementById('donutGreenSegment');
    if (donutGreen) {
      const circumference = 251;
      const greenOffset = circumference - (correct / total) * circumference;
      donutGreen.style.strokeDashoffset = greenOffset;
    }

    // Part analysis calculations
    const p5 = res.part5 || { correctCount: Math.round(correct * 0.4), totalQuestions: 30, accuracyPercentage: accuracy };
    const p6 = res.part6 || { correctCount: Math.round(correct * 0.2), totalQuestions: 16, accuracyPercentage: accuracy };
    const p7 = res.part7 || { correctCount: Math.round(correct * 0.4), totalQuestions: 54, accuracyPercentage: accuracy };

    document.getElementById('p5ScoreText').textContent = `${p5.correctCount}/${p5.totalQuestions} (${Math.round(p5.accuracyPercentage)}%)`;
    document.getElementById('p5ProgFill').style.width = `${Math.min(100, Math.round(p5.accuracyPercentage))}%`;

    document.getElementById('p6ScoreText').textContent = `${p6.correctCount}/${p6.totalQuestions} (${Math.round(p6.accuracyPercentage)}%)`;
    document.getElementById('p6ProgFill').style.width = `${Math.min(100, Math.round(p6.accuracyPercentage))}%`;

    document.getElementById('p7ScoreText').textContent = `${p7.correctCount}/${p7.totalQuestions} (${Math.round(p7.accuracyPercentage)}%)`;
    document.getElementById('p7ProgFill').style.width = `${Math.min(100, Math.round(p7.accuracyPercentage))}%`;
  }

  function escapeHtml(text) {
    if (!text) return '';
    return String(text)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }
});

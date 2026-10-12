// Admin Portal Client App - Google Stitch UI
document.addEventListener('DOMContentLoaded', () => {
  const tableBody = document.getElementById('examTableBody');
  const btnRefresh = document.getElementById('btnRefresh');
  const btnOpenImport = document.getElementById('btnOpenImport');
  const btnSidebarImport = document.getElementById('btnSidebarImport');
  const importModal = document.getElementById('importModal');
  const previewModal = document.getElementById('previewModal');
  const dropzone = document.getElementById('dropzone');
  const fileInput = document.getElementById('fileInput');
  const btnSubmitUpload = document.getElementById('btnSubmitUpload');
  const uploadFileInfo = document.getElementById('uploadFileInfo');
  const fileNameDisplay = document.getElementById('fileNameDisplay');
  const importErrorBox = document.getElementById('importErrorBox');
  const importErrorList = document.getElementById('importErrorList');
  const importSuccessBox = document.getElementById('importSuccessBox');
  const importSuccessDetails = document.getElementById('importSuccessDetails');
  const searchInput = document.getElementById('searchInput');
  const filterStatus = document.getElementById('filterStatus');
  const btnResetFilter = document.getElementById('btnResetFilter');

  let allExams = [];
  let selectedFile = null;

  // Initialize
  loadExams();

  btnRefresh.addEventListener('click', loadExams);
  if (btnSidebarImport) btnSidebarImport.addEventListener('click', () => btnOpenImport.click());

  // Search & Filter
  if (searchInput) {
    searchInput.addEventListener('input', applyFilters);
  }
  if (filterStatus) {
    filterStatus.addEventListener('change', applyFilters);
  }
  if (btnResetFilter) {
    btnResetFilter.addEventListener('click', () => {
      if (searchInput) searchInput.value = '';
      if (filterStatus) filterStatus.value = 'all';
      applyFilters();
    });
  }

  // Keyboard shortcut Ctrl+K
  window.addEventListener('keydown', (e) => {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
      e.preventDefault();
      searchInput.focus();
    }
  });

  // Modal close buttons
  document.querySelectorAll('.btnCloseModal').forEach(btn => {
    btn.addEventListener('click', () => {
      importModal.classList.remove('active');
      previewModal.classList.remove('active');
    });
  });

  // Open Import Modal
  btnOpenImport.addEventListener('click', () => {
    selectedFile = null;
    fileInput.value = '';
    uploadFileInfo.style.display = 'none';
    importErrorBox.style.display = 'none';
    importSuccessBox.style.display = 'none';
    btnSubmitUpload.disabled = true;
    btnSubmitUpload.innerHTML = '<i class="fa-solid fa-upload"></i> Bắt đầu Import';
    importModal.classList.add('active');
  });

  // Drag and Drop
  dropzone.addEventListener('click', () => fileInput.click());
  dropzone.addEventListener('dragover', (e) => {
    e.preventDefault();
    dropzone.classList.add('dragover');
  });
  dropzone.addEventListener('dragleave', () => dropzone.classList.remove('dragover'));
  dropzone.addEventListener('drop', (e) => {
    e.preventDefault();
    dropzone.classList.remove('dragover');
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileSelected(e.dataTransfer.files[0]);
    }
  });

  fileInput.addEventListener('change', () => {
    if (fileInput.files && fileInput.files[0]) {
      handleFileSelected(fileInput.files[0]);
    }
  });

  function handleFileSelected(file) {
    if (!file.name.endsWith('.docx')) {
      alert('Vui lòng chọn file Word định dạng .docx');
      return;
    }
    selectedFile = file;
    fileNameDisplay.textContent = file.name;
    uploadFileInfo.style.display = 'block';
    btnSubmitUpload.disabled = false;
    importErrorBox.style.display = 'none';
    importSuccessBox.style.display = 'none';
  }

  // Upload handler
  btnSubmitUpload.addEventListener('click', async () => {
    if (!selectedFile) return;

    btnSubmitUpload.disabled = true;
    btnSubmitUpload.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang phân tích...';
    importErrorBox.style.display = 'none';
    importSuccessBox.style.display = 'none';

    const formData = new FormData();
    formData.append('file', selectedFile);

    try {
      const res = await fetch('/api/admin/exams/upload', {
        method: 'POST',
        body: formData
      });

      const raw = await res.json();
      const payload = raw.result !== undefined ? raw.result : raw;
      const errorObj = raw.error;

      if (!res.ok || (raw.success === false && errorObj)) {
        // Validation errors
        if (payload && payload.errors && payload.errors.length > 0) {
          importErrorList.innerHTML = payload.errors.map(err => `<li>${escapeHtml(err)}</li>`).join('');
          importErrorBox.style.display = 'block';
        } else if (errorObj && errorObj.details) {
          const detailLines = errorObj.details.split('\n');
          importErrorList.innerHTML = detailLines.map(line => `<li>${escapeHtml(line)}</li>`).join('');
          importErrorBox.style.display = 'block';
        } else {
          const msg = (errorObj ? errorObj.message : '') || (payload ? payload.message : '') || 'Lỗi xử lý file';
          importErrorList.innerHTML = `<li>${escapeHtml(msg)}</li>`;
          importErrorBox.style.display = 'block';
        }
        btnSubmitUpload.disabled = false;
        btnSubmitUpload.innerHTML = '<i class="fa-solid fa-upload"></i> Thử lại';
        return;
      }

      // Success
      importSuccessDetails.textContent = `Đã nhập thành công đề "${payload.title}" với ${payload.totalQuestions} câu hỏi.`;
      importSuccessBox.style.display = 'block';
      btnSubmitUpload.innerHTML = '<i class="fa-solid fa-check"></i> Hoàn tất';

      setTimeout(() => {
        importModal.classList.remove('active');
        loadExams();
      }, 1500);

    } catch (err) {
      importErrorList.innerHTML = `<li>Không thể kết nối đến máy chủ: ${escapeHtml(err.message)}</li>`;
      importErrorBox.style.display = 'block';
      btnSubmitUpload.disabled = false;
      btnSubmitUpload.innerHTML = '<i class="fa-solid fa-upload"></i> Bắt đầu Import';
    }
  });

  // Load Exam List
  async function loadExams() {
    tableBody.innerHTML = `
      <tr>
        <td colspan="7" style="text-align:center; padding:3rem; color:var(--text-muted);">
          <i class="fa-solid fa-spinner fa-spin" style="margin-right:8px; font-size:18px;"></i> Đang tải dữ liệu đề thi...
        </td>
      </tr>
    `;

    try {
      const res = await fetch('/api/admin/exams');
      if (!res.ok) throw new Error('Không thể tải danh sách đề thi');
      const data = await res.json();
      const payload = data.result !== undefined ? data.result : data;
      allExams = Array.isArray(payload) ? payload : (payload.items || []);

      updateStats(allExams);
      applyFilters();

    } catch (err) {
      tableBody.innerHTML = `
        <tr>
          <td colspan="7" style="text-align:center; padding:2rem; color:#ef4444;">
            <i class="fa-solid fa-triangle-exclamation"></i> Lỗi tải danh sách: ${escapeHtml(err.message)}
          </td>
        </tr>
      `;
    }
  }

  function updateStats(exams) {
    const total = exams.length;
    const published = exams.filter(e => e.isPublished).length;
    const draft = total - published;

    document.getElementById('statPublishedBadge').textContent = published;
    document.getElementById('statDraftBadge').textContent = draft;

    const percent = total > 0 ? Math.round((published / total) * 100) : 0;
    document.getElementById('progressPercentage').textContent = `${percent}%`;
    document.getElementById('progressTextSub').textContent = `${published} trên ${total} bộ đề sẵn sàng khảo thí`;

    // SVG circle offset calculation (circumference is 220)
    const circleFill = document.getElementById('circleProgressFill');
    if (circleFill) {
      const circumference = 220;
      const offset = circumference - (percent / 100) * circumference;
      circleFill.style.strokeDashoffset = offset;
    }
  }

  function applyFilters() {
    const query = (searchInput ? searchInput.value : '').toLowerCase().trim();
    const status = filterStatus ? filterStatus.value : 'all';

    let filtered = allExams.filter(exam => {
      const matchQuery = exam.title.toLowerCase().includes(query) || (exam.description && exam.description.toLowerCase().includes(query));
      let matchStatus = true;
      if (status === 'published') matchStatus = exam.isPublished;
      if (status === 'draft') matchStatus = !exam.isPublished;
      return matchQuery && matchStatus;
    });

    renderTable(filtered);
  }

  function renderTable(exams) {
    if (exams.length === 0) {
      tableBody.innerHTML = `
        <tr>
          <td colspan="7" style="text-align:center; padding:3rem; color:var(--text-muted);">
            <div style="font-size:32px; margin-bottom:8px;"><i class="fa-regular fa-folder-open"></i></div>
            Chưa có đề thi nào phù hợp với bộ lọc.
          </td>
        </tr>
      `;
      document.getElementById('paginationSummary').textContent = 'Hiển thị 0 đề thi';
      return;
    }

    document.getElementById('paginationSummary').textContent = `Hiển thị 1 - ${exams.length} trong tổng số ${allExams.length} đề thi`;

    tableBody.innerHTML = exams.map((exam, index) => {
      const stt = String(index + 1).padStart(2, '0');
      const isPub = exam.isPublished;
      const statusBadge = isPub 
        ? `<span class="badge-status published">Đã xuất bản</span>`
        : `<span class="badge-status draft">Bản nháp</span>`;

      const iconClass = isPub ? 'exam-icon-box' : 'exam-icon-box draft';
      const icon = isPub ? '<i class="fa-solid fa-book-open"></i>' : '<i class="fa-regular fa-file-lines"></i>';

      const dateStr = exam.creationTime ? new Date(exam.creationTime).toLocaleDateString('vi-VN') : '14/10/2026';

      return `
        <tr>
          <td style="font-weight: 600; color: var(--text-muted);">${stt}</td>
          <td>
            <div class="exam-cell">
              <div class="${iconClass}">${icon}</div>
              <div>
                <div class="exam-meta-title">
                  <span>${escapeHtml(exam.title)}</span>
                  ${index === 2 ? '<span class="badge-new">Mới</span>' : ''}
                </div>
                <div class="exam-meta-sub">
                  Mã đề: RC-2024-${stt} • ${escapeHtml(exam.description || 'Chuẩn format ETS Reading')}
                </div>
              </div>
            </div>
          </td>
          <td>
            <div style="display:flex; align-items:center; gap:6px; color:var(--text-secondary); font-size:12px;">
              <i class="fa-regular fa-clock"></i>
              <span>${exam.durationMinutes} phút</span>
            </div>
          </td>
          <td>
            <span class="badge-count">${exam.totalQuestions} câu</span>
          </td>
          <td>${statusBadge}</td>
          <td>
            <div style="font-size:12px; color:var(--text-primary); font-weight:500;">${dateStr}</div>
            <div style="font-size:11px; color:var(--text-muted);">ThS. Thảo My</div>
          </td>
          <td>
            <div class="action-btns" style="justify-content: flex-end;">
              <button class="action-btn" title="Xem trước đề thi" onclick="previewExam(${exam.id})">
                <i class="fa-regular fa-eye"></i>
              </button>
              <button class="action-btn" title="Biên tập" onclick="editExam(${exam.id})">
                <i class="fa-regular fa-pen-to-square"></i>
              </button>
              <button class="action-btn" title="${isPub ? 'Hạ xuất bản (Đưa về nháp)' : 'Xuất bản đề thi'}" onclick="togglePublish(${exam.id}, ${isPub})">
                <i class="fa-solid ${isPub ? 'fa-toggle-on' : 'fa-toggle-off'}" style="font-size:18px; color:${isPub ? '#10b981' : '#94a3b8'};"></i>
              </button>
              <button class="action-btn delete" title="Xóa đề thi" onclick="deleteExam(${exam.id})">
                <i class="fa-regular fa-trash-can"></i>
              </button>
            </div>
          </td>
        </tr>
      `;
    }).join('');
  }

  // Toggle publish
  window.togglePublish = async function(id, currentStatus) {
    const actionName = currentStatus ? 'hạ xuất bản về bản nháp' : 'xuất bản đề thi này';
    if (!confirm(`Bạn có chắc chắn muốn ${actionName}?`)) return;

    try {
      const endpoint = currentStatus ? `/api/admin/exams/${id}/unpublish` : `/api/admin/exams/${id}/publish`;
      const res = await fetch(endpoint, { method: 'POST' });
      if (!res.ok) throw new Error('Không thể thay đổi trạng thái đề thi');
      loadExams();
    } catch (err) {
      alert(`Lỗi: ${err.message}`);
    }
  };

  // Delete Exam
  window.deleteExam = async function(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa đề thi này không? Hành động này không thể hoàn tác!')) return;

    try {
      const res = await fetch(`/api/admin/exams/${id}`, { method: 'DELETE' });
      if (!res.ok) throw new Error('Không thể xóa đề thi');
      loadExams();
    } catch (err) {
      alert(`Lỗi: ${err.message}`);
    }
  };

  // Edit Exam
  const editModal = document.getElementById('editModal');
  const editExamId = document.getElementById('editExamId');
  const editExamTitle = document.getElementById('editExamTitle');
  const editExamDuration = document.getElementById('editExamDuration');
  const editExamDesc = document.getElementById('editExamDesc');
  const editErrorBox = document.getElementById('editErrorBox');
  const btnSaveEditExam = document.getElementById('btnSaveEditExam');
  const btnEditFromPreview = document.getElementById('btnEditFromPreview');

  let currentPreviewExamId = null;

  window.editExam = function(id) {
    const exam = allExams.find(e => e.id === id);
    if (!exam) return;
    editExamId.value = exam.id;
    editExamTitle.value = exam.title || '';
    editExamDuration.value = exam.durationMinutes || 75;
    editExamDesc.value = exam.description || '';
    if (editErrorBox) editErrorBox.style.display = 'none';
    if (editModal) editModal.classList.add('active');
  };

  if (btnEditFromPreview) {
    btnEditFromPreview.addEventListener('click', () => {
      if (currentPreviewExamId) {
        previewModal.classList.remove('active');
        editExam(currentPreviewExamId);
      }
    });
  }

  if (btnSaveEditExam) {
    btnSaveEditExam.addEventListener('click', async () => {
      const id = parseInt(editExamId.value);
      const title = editExamTitle.value.trim();
      const duration = parseInt(editExamDuration.value);
      const desc = editExamDesc.value.trim();

      if (!title) {
        if (editErrorBox) {
          editErrorBox.textContent = 'Vui lòng nhập tiêu đề đề thi!';
          editErrorBox.style.display = 'block';
        }
        return;
      }

      btnSaveEditExam.disabled = true;
      btnSaveEditExam.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang lưu...';

      try {
        const res = await fetch(`/api/admin/exams/${id}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            id: id,
            title: title,
            durationMinutes: duration || 75,
            description: desc
          })
        });

        if (!res.ok) {
          const errData = await res.json().catch(() => ({}));
          throw new Error(errData.message || 'Không thể cập nhật đề thi');
        }

        if (editModal) editModal.classList.remove('active');
        alert('Cập nhật đề thi thành công!');
        loadExams();
      } catch (err) {
        if (editErrorBox) {
          editErrorBox.textContent = `Lỗi: ${err.message}`;
          editErrorBox.style.display = 'block';
        }
      } finally {
        btnSaveEditExam.disabled = false;
        btnSaveEditExam.innerHTML = '<i class="fa-solid fa-floppy-disk"></i> Lưu thay đổi';
      }
    });
  }

  // Preview Exam
  window.previewExam = async function(id) {
    currentPreviewExamId = id;
    const modalBody = document.getElementById('previewModalBody');
    modalBody.innerHTML = '<div style="text-align:center; padding:3rem;"><i class="fa-solid fa-spinner fa-spin"></i> Đang tải dữ liệu xem trước...</div>';
    previewModal.classList.add('active');

    try {
      const res = await fetch(`/api/admin/exams/${id}`);
      if (!res.ok) throw new Error('Không thể tải bản xem trước đề thi');
      const raw = await res.json();
      const exam = raw.result !== undefined ? raw.result : raw;

      document.getElementById('previewModalTitle').textContent = exam.title;
      document.getElementById('previewModalSub').textContent = `Tổng cộng: ${exam.totalQuestions} câu hỏi • Thời lượng: ${exam.durationMinutes} phút`;

      renderPreviewContent(exam);

    } catch (err) {
      modalBody.innerHTML = `<div style="color:#ef4444; padding:2rem; text-align:center;">Lỗi: ${err.message}</div>`;
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

  function renderPreviewContent(exam) {
    const modalBody = document.getElementById('previewModalBody');
    const standaloneQuestions = exam.standaloneQuestions || [];
    const groups = exam.groups || [];

    if (standaloneQuestions.length === 0 && groups.length === 0) {
      modalBody.innerHTML = '<div style="text-align:center; padding:3rem; color:var(--text-muted);"><i class="fa-regular fa-folder-open" style="font-size:32px; margin-bottom:8px; display:block;"></i>Đề thi chưa có câu hỏi nào.</div>';
      return;
    }

    let html = '';

    // Render Part 5: Standalone Questions
    if (standaloneQuestions.length > 0) {
      const minNum = standaloneQuestions[0].questionNumber;
      const maxNum = standaloneQuestions[standaloneQuestions.length - 1].questionNumber;
      html += `
        <div style="margin-bottom:28px;">
          <div style="display:flex; align-items:center; gap:8px; margin-bottom:14px;">
            <span style="background:#2563eb; color:white; font-size:11px; font-weight:700; padding:4px 12px; border-radius:999px;">PART 5</span>
            <span style="font-size:14px; font-weight:700; color:#1e293b;">Incomplete Sentences (Câu ${minNum} - ${maxNum})</span>
          </div>
          ${renderQuestionList(standaloneQuestions)}
        </div>
      `;
    }

    // Render Part 6 & Part 7: Question Groups with Passages
    if (groups.length > 0) {
      groups.forEach(group => {
        const passageItems = group.passageItems || group.passages || [];
        let passagesHtml = '';
        if (passageItems.length > 0) {
          passagesHtml = `
            <div style="background: white; border: 1px solid var(--border-color); border-radius: 10px; padding: 16px; margin-bottom: 16px;">
              <div style="font-size:11px; font-weight:700; color:#2563eb; text-transform:uppercase; margin-bottom:8px;">
                <i class="fa-regular fa-file-lines"></i> Đoạn văn đọc hiểu / Passage
              </div>
              ${passageItems.map(p => {
                if (p.type === 2 && p.imageUrl) {
                  return `<div style="text-align:center; margin:10px 0;"><img src="${p.imageUrl}" style="max-width:100%; max-height:300px; border-radius:8px; border:1px solid #e2e8f0;"></div>`;
                }
                return `<div style="font-size:13px; line-height:1.6; color:#334155; white-space:pre-wrap;">${escapeHtml(p.textContent || p.text || '')}</div>`;
              }).join('')}
            </div>
          `;
        }

        html += `
          <div style="margin-bottom:28px;">
            <div style="display:flex; align-items:center; gap:8px; margin-bottom:14px;">
              <span style="background:#0284c7; color:white; font-size:11px; font-weight:700; padding:4px 12px; border-radius:999px;">PART ${group.partNumber}</span>
              <span style="font-size:14px; font-weight:700; color:#1e293b;">Câu ${group.fromQuestionNumber} - ${group.toQuestionNumber}</span>
            </div>
            ${passagesHtml}
            ${renderQuestionList(group.questions || [])}
          </div>
        `;
      });
    }

    modalBody.innerHTML = html;
  }

  function renderQuestionList(questions) {
    return questions.map(q => {
      const qText = q.content || q.text || '';
      const correctAns = normalizeOption(q.correctOption || q.correctAnswer);
      const options = q.options || [];

      const optionsHtml = options.map(opt => {
        const optLabel = normalizeOption(opt.label);
        const isCorrect = (optLabel && optLabel === correctAns) || opt.isCorrect;
        const optText = opt.content || opt.text || '';
        return `
          <div style="padding:8px 12px; border-radius:6px; font-size:12px; border:1.5px solid ${isCorrect ? '#10b981' : '#e2e8f0'}; background:${isCorrect ? '#ecfdf5' : 'white'}; display:flex; align-items:center; gap:8px;">
            <strong style="color:${isCorrect ? '#059669' : '#64748b'};">${optLabel || opt.label}.</strong>
            <span style="color:#1e293b; font-weight:${isCorrect ? '600' : '400'};">${escapeHtml(optText)}</span>
            ${isCorrect ? '<i class="fa-solid fa-check" style="margin-left:auto; color:#10b981;"></i>' : ''}
          </div>
        `;
      }).join('');

      return `
        <div style="background:white; border:1px solid var(--border-color); border-radius:10px; padding:16px; margin-bottom:14px;">
          <div style="display:flex; justify-content:space-between; margin-bottom:10px;">
            <span style="font-weight:700; color:#0f172a; font-size:13px;">Câu ${q.questionNumber}.</span>
            <span style="font-size:11px; color:#15803d; background:#dcfce7; padding:3px 10px; border-radius:999px; font-weight:700;">
              Đáp án đúng: ${correctAns || 'Chưa cập nhật'}
            </span>
          </div>
          <div style="font-size:13px; color:#1e293b; margin-bottom:12px; line-height:1.5;">${escapeHtml(qText)}</div>
          <div style="display:grid; grid-template-columns:1fr 1fr; gap:8px;">${optionsHtml}</div>
          ${q.explanation ? `
            <div style="margin-top:10px; font-size:12px; color:#475569; background:#f8fafc; border-left:3px solid #3b82f6; padding:8px 12px; border-radius:0 6px 6px 0;">
              <strong style="color:#1e40af;"><i class="fa-regular fa-lightbulb"></i> Giải thích:</strong> ${escapeHtml(q.explanation)}
            </div>
          ` : ''}
        </div>
      `;
    }).join('');
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

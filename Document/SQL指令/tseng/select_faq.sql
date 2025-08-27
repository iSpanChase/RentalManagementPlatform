SELECT 
    c.faq_categories_id       AS CategoryID,
    c.name                    AS CategoryName,
    a.faq_articles_id         AS ArticleID,
    a.title                   AS ArticleTitle,
    a.summary                 AS ArticleSummary,
    a.view_count              AS Views,
    a.helpful_yes             AS HelpfulYes,
    a.helpful_no              AS HelpfulNo,
    f.faq_feedback_id         AS FeedbackID,
    f.sentiment               AS FeedbackSentiment,
    f.reason                  AS FeedbackReason,
    f.contact_email           AS FeedbackEmail,
    f.escalated_to_ticket     AS Escalated,
    f.created_at              AS FeedbackDate
FROM FAQ_CATEGORIES c
LEFT JOIN FAQ_ARTICLES a 
       ON c.faq_categories_id = a.category_id
LEFT JOIN FAQ_FEEDBACK f 
       ON a.faq_articles_id = f.article_id
ORDER BY 
    c.faq_categories_id,
    a.faq_articles_id,
    f.created_at;
